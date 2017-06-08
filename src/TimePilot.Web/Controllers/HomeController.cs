using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TimePilot.Web.Models;
using TimePilot.Web.ViewModels;
using TimePilot.DataAccess.Repository;
using TimePilot.Entities;

namespace TimePilot.Controllers
{
    public class HomeController : Controller
    {
        ApiHelper apiHelper = new ApiHelper();
        private string projectJson;
        private string storyJson;
        ProjectViewModel ProjectVM = new ProjectViewModel();
        StoryViewModel StoryVM = new StoryViewModel();
        ResourceCapacityViewModel mResourceViewModel = new ResourceCapacityViewModel();
        ResultsViewModel ResultsVM = new ResultsViewModel();
        public static int [] storypointallocation;
        public static float totalAvailablility;
        public static float AvgCapactiyperWeek;
        public static float totalStoryPoints;
        private static int hoursPerDay = 8;
        List<TimePilot.Entities.Story> stories = new List<TimePilot.Entities.Story>();        
        List<Project> projects = new List<Project>();
        public static string SelectedProject;

        ProjectRepository ProjDB = new ProjectRepository();
        StoryRepository StoryDB = new StoryRepository();

        public void originateResourceCapacity()
        {
            Member member = new Member();
            Sprint sprint = new Sprint();
            List<Sprint> sprintList = new List<Sprint>();
            List<Member> memberlist = new List<Member>();
            memberlist.Add(member);
            sprint.members = memberlist;
            sprintList.Add(sprint);
            mResourceViewModel.sprints = sprintList;
        }

        public void bindStoryDataToViewModel()
        {
            stories = StoryDB.GetAllByForeignId(SelectedProject);
            StoryVM.StoryList = stories;
        }

        public void bindProjectDataToViewModel()
        {
            projects = ProjDB.GetAll();
            ProjectVM.ProjectList = projects;
        }

        public void receiveProjectData()
        {
            this.projectJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            this.storyJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(story,improvement)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,id,description,summary&maxResults=1000");
        }

        public ActionResult Index()
        {
            /* Populate should only be called by a button, is called every
             * page load for now until button is implemented             */
            ProjectPopulate();
            /*-----------------------------------------------------------*/
            bindProjectDataToViewModel();
            return View(ProjectVM);
        }

        [HttpPost]
        public ActionResult Index(ProjectViewModel model)
        {
            SelectedProject = model.SelectedProject;
            // On button click form is posted and redirects to Story view
            return RedirectToAction("Story", "Home");
        }

        /* Iterates through projects grabbed from API call
         * If it's in the database, do nothing
         * If it's not in the database, add it */
        private void ProjectPopulate()
        {
            receiveProjectData();
            projects = apiHelper.parseProjectData(projectJson);

            Project temp;
            for (int i = 0; i < projects.Count; i++)
            {
                temp = ProjDB.GetById(projects[i]);
                if (temp.ProjectKey == projects[i].ProjectKey)
                {
                    return;
                }
                else
                {
                    ProjDB.Add(projects[i]);
                }
            }
        }

        public void setHoursEstimationValues(ResultsViewModel model)
        {
            int SumOfTotalDays = 0;
            for (int i = 0; i < model.storypointAllocation.Length; i++)
            {
                SumOfTotalDays = SumOfTotalDays + model.Total[i];
            }
            model.TotalHours = SumOfTotalDays * hoursPerDay;                                    
            model.ReleaseAndHardening = model.SprintLength;
            model.TotalDevQA = model.TotalHours * (1F + model.Contingency/100F);
            model.AvgCapacitiyperWeek = AvgCapactiyperWeek;
            model.TotalWeeks = model.TotalDevQA / AvgCapactiyperWeek;
            model.ProjectDurationWeeks = model.ReleaseAndHardening + model.TotalWeeks;
        }

        public void setVelocityValidationValues(ResultsViewModel model)
        {
            model.totalStoryPoints = totalStoryPoints;
            model.totalPointsVelocity = totalStoryPoints * (1F + model.Contingency / 100F);
            
            if (model.TeamVelocity > 0)
            {
                model.TotalSprints = model.totalStoryPoints / model.TeamVelocity;
            }
            model.TotalWeeksVelocity = model.TotalSprints * model.SprintLength;
            model.projectDurationWeeksVelocity = model.TotalWeeksVelocity + model.ReleaseAndHardening;
        }

        public ActionResult Story()
        {
            //StoryPopulate();
            ModelState.Clear();
            bindStoryDataToViewModel();
            sumStoryPoints(StoryVM);
            if (StoryVM.StorypointSum != null)
            {
                storypointallocation = StoryVM.StorypointSum;
            }
            calculateTotalStoryPoints(StoryVM);
            return View(StoryVM);
        }

        [HttpPost]
        public ActionResult Story(StoryViewModel model, string command)
        {
            ModelState.Clear();
            sumStoryPoints(model);
            if (model.StorypointSum != null)
            {
                storypointallocation = model.StorypointSum;
            }
            calculateTotalStoryPoints(model);
            totalStoryPoints = model.totalNumberStoryPoints;

            if (command != null && command.Equals("Apply Changes"))
            {
                StoryUpdate(model);
            }

            if (command != null && command.Equals("Resource Capacity Page"))
            {
                return RedirectToAction("Resource", "Home");
            }
            return View(model);
        }

        public ActionResult StoryPopulate()
        {
            receiveStoryData();
            stories = apiHelper.parseStoryData(storyJson);
            TimePilot.Entities.Story temp;
            for (int i = 0; i < stories.Count; i++)
            {
                stories[i].ProjectKey = SelectedProject;
                temp = StoryDB.GetById(stories[i]);
                if (temp.ProjectKey == null)
                {
                    StoryDB.Add(stories[i]);
                }
            }
            return RedirectToAction("Story");
        }

        [HttpPost]
        public ActionResult StoryUpdate(StoryViewModel model)
        {
            foreach (var story in model.StoryList)
            {
                StoryDB.Update(story);
            }
            return RedirectToAction("Story");
        }

        [HttpPost]
        public ActionResult StoryDelete(int[] id)
        {
            foreach (var item in id)
            {
                StoryDB.Delete(item);
            }
            return RedirectToAction("Story");
        }

        public void calculateTotalStoryPoints (StoryViewModel model)
        {
            int sumofPoints = 0;

            if (model.StoryList != null)
            {

                for (int i = 0; i < model.StoryList.Count; i++)
                {
                    sumofPoints = sumofPoints + model.StoryList[i].StoryPoints;
                }
                model.totalNumberStoryPoints = sumofPoints;
            }
        }

        public ActionResult Resource()
        {
            mResourceViewModel.roleList = createRoleList();
            
            
                originateResourceCapacity();
            
            return View(mResourceViewModel);
        }

        [HttpPost]
        public ActionResult Resource(ResourceCapacityViewModel RCModel, string ddlRole, string command)
        {
            ModelState.Clear();            
            RCModel.roleList = createRoleList();
            calculateAvailability(RCModel);            

            if (RCModel.memberIndex != null)
            {
                
                string indexString = RCModel.memberIndex;
                int SprintIndex = int.Parse(indexString[0].ToString());
                int MemberIndex = int.Parse(indexString[1].ToString());
                setDefaultValues(RCModel, SprintIndex, MemberIndex);
            }
            if (command != null && command.Equals("Add Member"))
            {
                
                Member member = new Member();
                RCModel.sprints[RCModel.buttonIndex].members.Add(member);

            }

            if (command != null && command.Equals("Add Sprint"))
            {
                
                Sprint sprint = new Sprint();
                Member member = new Member();
                List<Member> memberList = new List<Member>();
                RCModel.sprints.Add(sprint);
                RCModel.sprints[RCModel.sprints.Count - 1].members = memberList;
                RCModel.sprints[RCModel.sprints.Count - 1].members.Add(member);

            }

            if (command != null && command.Equals("Delete Selected"))
            {
                
                deleteSelectedMembers(RCModel);
            }

            if (command != null && command.Equals("Delete Sprint"))
            {
                
                deleteSprint(RCModel);

            }
            calculateRCMainValues(RCModel);
            totalAvailablility = RCModel.totalDevCapacity;
            AvgCapactiyperWeek = RCModel.avgPerWeek;
            return View(RCModel);
        }

        public void calculateTotalDays(ResultsViewModel model)
        {
            for (int i = 0; i < model.storypointAllocation.Length; i++)
            {
                    model.Total[i] = model.numberOfStories[i] * model.DaysPerPt[i];               
            }
        }

        public ActionResult Result()
        {
            ResultsVM.sprintLengthList = createSprintLengthList();
            ResultsVM.DaysPerPt = new int[6];
            ResultsVM.Total = new int[6];
            ResultsVM.numberOfStories = new int[6];
            if (storypointallocation != null)
            {
                ResultsVM.numberOfStories = storypointallocation;
            }

            ResultsVM.storypointAllocation = new int[6];
            ResultsVM.storypointAllocation[0] = 1;
            ResultsVM.storypointAllocation[1] = 3;
            ResultsVM.storypointAllocation[2] = 5;
            ResultsVM.storypointAllocation[3] = 8;
            ResultsVM.storypointAllocation[4] = 13;
            ResultsVM.storypointAllocation[5] = 21;
            setHoursEstimationValues(ResultsVM);
            setVelocityValidationValues(ResultsVM);

            return View(ResultsVM);
        }

        [HttpPost]
        public ActionResult Result(ResultsViewModel model)
        {
            ModelState.Clear();
            model.sprintLengthList = createSprintLengthList();
            calculateTotalDays(model);
            setHoursEstimationValues(model);
            setVelocityValidationValues(model);

            return View(model);
        }

        private void deleteSelectedMembers(ResourceCapacityViewModel model)
        {
            for (int i = 0; i < model.sprints.Count; i++)
            {
                for (int j = 0; j < model.sprints[i].members.Count; j++)
                {
                    if (model.sprints[i].members.Count > 1 && model.sprints[i].members[j].isSelectedToDelete)
                    {
                        model.sprints[i].members.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private void deleteSprint (ResourceCapacityViewModel model)
        {
            if (model.sprints.Count > 1)
            {
                model.sprints.RemoveAt(model.buttonIndex);
            }

        }

        public void calculateAvailability(ResourceCapacityViewModel model)
        {
            for (int i = 0; i < model.sprints.Count; i++)
            {
                for (int j = 0; j < model.sprints[i].members.Count; j++)
                {
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours - model.sprints[i].members[j].timeOff;
                }
            }
        }      

        public void calculateRCMainValues(ResourceCapacityViewModel model)
        {
            for (int i = 0; i < model.sprints.Count; i++)
            {
                for (int j = 0; j < model.sprints[i].members.Count; j++)
                {
                    model.totalDevCapacity = model.totalDevCapacity + model.sprints[i].members[j].totalAvailable;
                }
            }
            model.avgPerSprint = model.totalDevCapacity / model.sprints.Count;
            model.avgPerWeek = model.avgPerSprint / 2;
        }

        //i = sprint index j = member index
        public void setDefaultValues(ResourceCapacityViewModel model, int i, int j)
        {
            switch (model.sprints[i].members[j].role)
            {
                case "leadDev":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 50;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 6;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "seniorDev":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 70;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 5;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "intermediateDev":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 80;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 4;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "juniorDev":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 90;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 4;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "leadQA":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 50;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 6;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "intermediateQA":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 70;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 5;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "seniorQA":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 80;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 4;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;

                case "juniorQA":
                    model.sprints[i].members[j].sprintDays = 10;
                    model.sprints[i].members[j].percentWork = 90;
                    model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                    model.sprints[i].members[j].standupDuration = 0.25F;
                    model.sprints[i].members[j].standUps = model.sprints[i].members[j].sprintDays * model.sprints[i].members[j].standupDuration;
                    model.sprints[i].members[j].misc = 4;
                    model.sprints[i].members[j].nonDevHours = model.sprints[i].members[j].misc + model.sprints[i].members[j].standUps;
                    model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                    break;
            }

            model.memberIndex = "00";
            

        }
       
        private void sumStoryPoints(StoryViewModel model)
        {
            int point1 = 0;
            int point3 = 0;
            int point5 = 0;
            int point8 = 0;
            int point13 = 0;
            int point21 = 0;
            int noneOftheAbove = 0;
            int[] pointArray = new int[7];

            if (model.StoryList != null)
            {
                for (int i = 0; i < model.StoryList.Count; i++)
                {
                    switch (model.StoryList[i].StoryPoints)
                    {
                        case 1:
                            point1 = point1 + 1;
                            break;
                        case 3:
                            point3 = point3 + 1;
                            break;
                        case 5:
                            point5 = point5 + 1;
                            break;
                        case 8:
                            point8 = point8 + 1;
                            break;
                        case 13:
                            point13 = point13 + 1;
                            break;
                        case 21:
                            point21 = point21 + 1;
                            break;
                        default:
                            noneOftheAbove = noneOftheAbove + 1;
                            break;
                    }
                }

            }
            pointArray[0] = point1;
            pointArray[1] = point3;
            pointArray[2] = point5;
            pointArray[3] = point8;
            pointArray[4] = point13;
            pointArray[5] = point21;
            pointArray[6] = noneOftheAbove;

            model.StorypointSum = pointArray;
        }

        private IEnumerable<SelectListItem> createSprintLengthList()
        {
            List<SelectListItem> mySprintLengthList = new List<SelectListItem>();
            SelectListItem sprintLength2 = new SelectListItem() { Text = "2", Value = "2"};
            SelectListItem sprintLength3 = new SelectListItem() { Text = "3", Value = "3"};
            mySprintLengthList.Add(sprintLength2);
            mySprintLengthList.Add(sprintLength3);

            IEnumerable<SelectListItem> myEnumSprintLengthList = mySprintLengthList;
            return myEnumSprintLengthList;
        }

        private IEnumerable<SelectListItem> createRoleList()
        {
            List<SelectListItem> myRoleList = new List<SelectListItem>();
            SelectListItem LeadDev = new SelectListItem() { Text = "Lead Dev", Value = "leadDev"};
            SelectListItem SeniorDev = new SelectListItem() { Text = "Senior Dev", Value = "seniorDev"};
            SelectListItem IntermediateDev = new SelectListItem() { Text = "Intermediate Dev", Value = "intermediateDev"};
            SelectListItem JuniorDev = new SelectListItem() { Text = "Junior Dev", Value = "juniorDev"};
            SelectListItem LeadQA = new SelectListItem() { Text = "Lead QA", Value = "leadQA"};
            SelectListItem SeniorQA = new SelectListItem() { Text = "Senior QA", Value = "seniorQA"};
            SelectListItem IntermediateQA = new SelectListItem() { Text = "Intermediate QA", Value = "intermediateQA"};
            SelectListItem JuniorQA = new SelectListItem() { Text = "Junior QA", Value = "juniorQA"};

            myRoleList.Add(LeadDev);
            myRoleList.Add(SeniorDev);
            myRoleList.Add(IntermediateDev);
            myRoleList.Add(JuniorDev);
            myRoleList.Add(LeadQA);
            myRoleList.Add(SeniorQA);
            myRoleList.Add(IntermediateQA);
            myRoleList.Add(JuniorQA);

            IEnumerable<SelectListItem> myEnumRoleList = myRoleList;
            return myEnumRoleList;
        }
    }
}





