using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TimePilot.Web.ViewModels;
using TimePilot.DataAccess.Repository;
using TimePilot.Entities;

namespace TimePilot.Controllers
{
    public class HomeController : Controller
    {
        private static ApiHelper apiHelper = new ApiHelper();
        private string projectJson;
        private string storyJson;

        public static float totalAvailablility;
        public static float totalDevCapacity;
        public static float totalStoryPoints;

        public static int[] StoryPointAllocation;
        private static string SelectedProject = "";

        List<Story> stories = new List<Story>();        
        List<Project> projects = new List<Project>();

        ProjectViewModel ProjectVM = new ProjectViewModel();
        StoryViewModel StoryVM = new StoryViewModel();
        ResourceCapacityViewModel ResourceVM = new ResourceCapacityViewModel();
        ResultsViewModel ResultsVM = new ResultsViewModel();

        ProjectRepository ProjDB = new ProjectRepository();
        StoryRepository StoryDB = new StoryRepository();
        SprintRepository SprintDB = new SprintRepository();
        MemberRepository MemberDB = new MemberRepository();

        public void receiveProjectData()
        {
            projectJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            storyJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(story,improvement)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,id,description,summary&maxResults=1000");
        }

        public ActionResult Index()
        {
            projects = ProjDB.GetAll();
            ProjectVM.ProjectList = projects;
            return View(ProjectVM);
        }

        [HttpPost]
        public ActionResult Index(ProjectViewModel m)
        {
            ProjectVM = m;
            SelectedProject = m.SelectedProject;
            // On button click form is posted and redirects to Story view
            if (m.SelectedProject == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Story", "Home");
            }
        }

        /* Iterates through projects grabbed from API call
         * If it's in the database, do nothing
         * If it's not in the database, add it */
        public ActionResult ProjectPopulate()
        {
            receiveProjectData();
            projects = apiHelper.parseProjectData(projectJson);

            Project temp;
            for (int i = 0; i < projects.Count; i++)
            {
                temp = ProjDB.GetById(projects[i]);
                if (!(temp.ProjectKey == projects[i].ProjectKey))
                    ProjDB.Add(projects[i]);
                }
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 0, VaryByParam = "none", NoStore = true)]
        public ActionResult Story()
        {
            stories = StoryDB.GetAllByForeignId(SelectedProject);
            StoryVM.StoryList = stories;
            return View(StoryVM);
        }

        public ActionResult StoryPopulate()
        {
            receiveStoryData();
            stories = apiHelper.parseStoryData(storyJson);
            Story temp;
            for (int i = 0; i < stories.Count; i++)
            {
                // TODO: Bring this logic into repository
                stories[i].ProjectKey = SelectedProject;
                temp = StoryDB.GetById(stories[i]);
                if (temp.ProjectKey == null)
                {
                    StoryDB.Add(stories[i]);
                }
                else
                {
                    StoryDB.Update(stories[i]);
                }
            }
            return RedirectToAction("Story");
        }

        public ActionResult StoryBringBackDeleted()
        {
            receiveStoryData();
            stories = apiHelper.parseStoryData(storyJson);
            Story temp;
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
        public ActionResult StoryUpdate(StoryViewModel m)
        {
            StoryVM = m;
            if (m.StoryList != null)
            {
                foreach (var s in m.StoryList)
                {
                    s.ProjectKey = SelectedProject;
                    StoryDB.Update(s);
                }
            }
            totalStoryPoints = m.totalStoryPoints;
            sortStoryPointsIntoBuckets();
            return RedirectToAction("Resource");
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

        private void sortStoryPointsIntoBuckets()
        {
            int point1 = 0;
            int point3 = 0;
            int point5 = 0;
            int point8 = 0;
            int point13 = 0;
            int point21 = 0;
            int noneOftheAbove = 0;
            int[] pointArray = new int[7];

            if (StoryVM.StoryList != null)
            {
                for (int i = 0; i < StoryVM.StoryList.Count; i++)
                {
                    switch (StoryVM.StoryList[i].StoryPoints)
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

            StoryPointAllocation = pointArray;
        }

        public ActionResult Resource()
        {
            List<Member> currentMembers = new List<Member>();
            ResourceVM.members = currentMembers;
            ResourceVM.sprints = SprintDB.GetAllByForeignId(SelectedProject);
            if (ResourceVM.sprints.Count == 0)
            {
                Sprint initSprint = new Sprint();
                ResourceVM.sprints.Add(initSprint);
            }
            for(int i = 0; i < ResourceVM.sprints.Count; i++)
            {
                currentMembers = MemberDB.GetAllByForeignId(ResourceVM.sprints[i].SprintID);
                if (currentMembers.Count >= 1)
                {
                    for (int j = 0; j < currentMembers.Count; j++)
                    {
                        ResourceVM.members.Add(currentMembers[j]);
                    }
                }
                else
                {
                    Member initMember = new Member();
                    ResourceVM.members.Add(initMember);
                }
            }
            return View(ResourceVM);
        }

        [HttpPost]
        public ActionResult ResourceUpdate(ResourceCapacityViewModel m)
        {
            ResourceVM = m;
            int dbCurrentSprintID = 0;
            int tempCurrentSprintID = 0;
            for (int i=0; i < m.sprints.Count; i++)
            {
                // Grab working reference to current sprint -- this ID is different than database ID
                tempCurrentSprintID = m.sprints[i].PageID;

                // Save Sprint to DB
                m.sprints[i].ProjectKey = SelectedProject;
                dbCurrentSprintID = SprintDB.Add(m.sprints[i]);

                if(m.members != null && m.members.Count >=1)
                {
                    for (int j = 0; j < ResourceVM.members.Count; j++)
                    {
                        if (m.members[j].PageID != -1 && m.members[j].PageID == tempCurrentSprintID)
                        {
                            m.members[j].SprintID = dbCurrentSprintID;
                            m.members[j].MemberID = MemberDB.Add(m.members[j]);
                        }
                    }
                }
            }
            totalDevCapacity = m.totalDevCapacity;
            return RedirectToAction("Resource");
        }

        [HttpPost]
        public ActionResult ResourceDelete(int[] id)
        {
            foreach (var item in id)
            {
                MemberDB.Delete(item);
            }
            return RedirectToAction("Resource");
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
            ResultsVM.DaysPerPt = new int[6];
            ResultsVM.Total = new int[6];
            ResultsVM.numberOfStories = new int[6];
            if (StoryPointAllocation != null)
            {
                ResultsVM.numberOfStories = StoryPointAllocation;
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
            calculateTotalDays(model);
            setHoursEstimationValues(model);
            setVelocityValidationValues(model);

            return View(model);
        }

        public void setHoursEstimationValues(ResultsViewModel model)
        {
            int SumOfTotalDays = 0;
            for (int i = 0; i < model.storypointAllocation.Length; i++)
            {
                SumOfTotalDays = SumOfTotalDays + model.Total[i];
            }
            model.TotalHours = SumOfTotalDays * 8;
            model.ReleaseAndHardening = model.SprintLength;
            model.TotalDevQA = model.TotalHours * (1F + model.Contingency / 100F);
            model.AvgCapacityPerWeek = totalDevCapacity/2;
            model.TotalWeeks = model.TotalDevQA / (totalDevCapacity / 2);
            model.ProjectDurationWeeks = model.ReleaseAndHardening + model.TotalWeeks;
        }

        public void setVelocityValidationValues(ResultsViewModel model)
        {
            model.totalStoryPoints = totalStoryPoints;
            model.totalPointsVelocity = totalStoryPoints * (1F + model.Contingency / 100F);

            if (model.TeamVelocity > 0)
            {
                model.TotalSprints = model.totalPointsVelocity / model.TeamVelocity;
            }
            model.TotalWeeksVelocity = model.TotalSprints * model.SprintLength;
            model.projectDurationWeeksVelocity = model.TotalWeeksVelocity + model.ReleaseAndHardening;
        }

        /*private void deleteSelectedMembers(ResourceCapacityViewModel model)
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
        }*/

        /*private void deleteSprint (ResourceCapacityViewModel model)
        {
            if (model.sprints.Count > 1)
            {
                model.sprints.RemoveAt(model.buttonIndex);
            }

        }*/

        /* public void calculateAvailability(ResourceCapacityViewModel model)
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
        }      */

        /*public void calculateRCMainValues(ResourceCapacityViewModel model)
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
        }*/

        /* public void setDefaultValues(ResourceCapacityViewModel model, int i, int j)
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

            model.memberIndex = null;
            

        }*/

        /*private IEnumerable<SelectListItem> createSprintLengthList()
        {
            List<SelectListItem> mySprintLengthList = new List<SelectListItem>();
            SelectListItem sprintLength2 = new SelectListItem() { Text = "2", Value = "2"};
            SelectListItem sprintLength3 = new SelectListItem() { Text = "3", Value = "3"};
            mySprintLengthList.Add(sprintLength2);
            mySprintLengthList.Add(sprintLength3);

            IEnumerable<SelectListItem> myEnumSprintLengthList = mySprintLengthList;
            return myEnumSprintLengthList;
        }*/

        /*private IEnumerable<SelectListItem> createRoleList()
        {
            List<SelectListItem> myRoleList = new List<SelectListItem>();
            SelectListItem LeadDev = new SelectListItem() { Text = "Lead Dev", Value = "leadDev"};
            SelectListItem SeniorDev = new SelectListItem() { Text = "Senior Dev", Value = "seniorDev"};
            SelectListItem IntermediateDev = new SelectListItem() { Text = "Dev", Value = "intermediateDev"};
            SelectListItem JuniorDev = new SelectListItem() { Text = "Junior Dev", Value = "juniorDev"};
            SelectListItem LeadQA = new SelectListItem() { Text = "Lead QA", Value = "leadQA"};
            SelectListItem SeniorQA = new SelectListItem() { Text = "Senior QA", Value = "seniorQA"};
            SelectListItem IntermediateQA = new SelectListItem() { Text = "QA", Value = "intermediateQA"};
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
        }*/
    }
}





