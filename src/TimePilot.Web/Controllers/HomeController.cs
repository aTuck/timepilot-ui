using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TimePilot.Models;
using TimePilot.ViewModels;

namespace TimePilot.Controllers
{
    public class HomeController : Controller
    {
        ApiHelper myhelper = new ApiHelper();
        private string projectJson;
        private string storyJson;
        ProjectViewModel mProjectViewModel = new ProjectViewModel();
        StoryViewModel mStoryViewModel = new StoryViewModel();
        ResourceCapacityViewModel mResourceViewModel = new ResourceCapacityViewModel();
        IEnumerable<SelectListItem> roleList;
        private static int hoursPerDay = 8;
        List<Project> projects = new List<Project>();
        List<Story> stories = new List<Story>();
        public static string SelectedProject;


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
            mStoryViewModel.mStoryList = stories;
        }

        public void bindProjectDataToViewModel()
        {
            mProjectViewModel.ProjectList = projects;
        }

        public void receiveProjectData()
        {
            this.projectJson = myhelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            this.storyJson = myhelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(story,improvement)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,id,description,summary&maxResults=1000");
        }


        public ActionResult Index()
        {
            receiveProjectData();
            projects = myhelper.parseProjectData(projectJson);
            bindProjectDataToViewModel();
            return View(mProjectViewModel);
        }

        [HttpPost]
        public ActionResult Index(ProjectViewModel model)
        {
            SelectedProject = model.SelectedProject;
            // On button click form is posted and redirects to Story view
            if (model.SelectedProject == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Story", "Home");
            }
        }

        public ActionResult Story()
        {
            receiveStoryData();
            stories = myhelper.parseStoryData(storyJson);
            convertStoryPointToInt(stories);
            bindStoryDataToViewModel();
            sumStoryPoints(mStoryViewModel);
            return View(mStoryViewModel);
        }



        [HttpPost]
        public ActionResult Story(StoryViewModel modelmodel)
        {
            ModelState.Clear();
            modelmodel.mStoryList = deleteSelectedStories(modelmodel);
            sumStoryPoints(modelmodel);
            return View(modelmodel);
        }

        [HttpPost]
        public ActionResult Resource(ResourceCapacityViewModel RCModel, String command, int buttonIndex)
        {
            ModelState.Clear();
            RCModel.roleList = createRoleList();
            setDefaultValues(RCModel);

            if (command != null && command.Equals("Add Member"))
            {

                Member member = new Member();
                RCModel.sprints[buttonIndex].members.Add(member);

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

            return View(RCModel);
        }

        public ActionResult Resource()
        {
            originateResourceCapacity();
            mResourceViewModel.roleList = createRoleList();
            return View(mResourceViewModel);
        }






        private List<Story> deleteSelectedStories(StoryViewModel model)
        {
            List<Story> myList = model.mStoryList;
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i].isSelectedToDelete)
                {
                    myList.RemoveAt(i);
                    i--;
                }
            }
            return myList;
        }

        public void setDefaultValues(ResourceCapacityViewModel model)
        {
            for (int i = 0; i < model.sprints.Count; i++)
            {


                for (int j = 0; j < model.sprints[i].members.Count; j++)
                {

                    if (model.sprints[i].members[j].role != "Select Role")
                    {

                        switch (model.sprints[i].members[j].role)
                        {

                            case "leadDev":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;

                            case "seniorDev":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;

                            case "intermediateDev":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;
                            case "juniorDev":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 80;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;

                            case "leadQA":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;

                            case "intermediateQA":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;
                            case "seniorQA":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;

                            case "juniorQA":

                                model.sprints[i].members[j].sprintDays = 10;
                                model.sprints[i].members[j].percentWork = 50;
                                model.sprints[i].members[j].totalHours = model.sprints[i].members[j].sprintDays * (model.sprints[i].members[j].percentWork / 100) * hoursPerDay;
                                model.sprints[i].members[j].standupDuration = 0.25F;
                                model.sprints[i].members[j].standUps = 1;
                                model.sprints[i].members[j].misc = 6;
                                model.sprints[i].members[j].nonDevHours = 6;
                                model.sprints[i].members[j].totalAvailable = model.sprints[i].members[j].totalHours - model.sprints[i].members[j].nonDevHours;
                                break;
                        }
                    }

                }
            }
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
            for (int i = 0; i < model.mStoryList.Count; i++)
            {
                switch (model.mStoryList[i].IntStoryPoint)
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
            pointArray[0] = point1;
            pointArray[1] = point3;
            pointArray[2] = point5;
            pointArray[3] = point8;
            pointArray[4] = point13;
            pointArray[5] = point21;
            pointArray[6] = noneOftheAbove;

            model.StorypointSum = pointArray;
            if (noneOftheAbove > 0)
            {
                model.ZeroStoryFlag = true;
            }
            else
            {
                model.ZeroStoryFlag = false;
            }
        }

        private void convertStoryPointToInt(List<Story> mlist)
        {
            for (int i = 0; i < mlist.Count; i++)
            {
                if (mlist[i].StoryPoint != null)
                {
                    mlist[i].IntStoryPoint = Convert.ToInt32(mlist[i].StoryPoint);
                }
                else
                {
                    mlist[i].IntStoryPoint = 0;
                }
            }
        }

        private IEnumerable<SelectListItem> convertProjectListToIEnum(List<Project> projectList)
        {
            List<SelectListItem> selectListItemList = new List<SelectListItem>();
            for (int i = 0; i < projectList.Count; i++)
            {
                SelectListItem selectListItem = new SelectListItem()
                {
                    Text = projectList[i].Key + ": " + projectList[i].Name,
                    Value = projectList[i].Key,
                    Selected = false
                };
                selectListItemList.Add(selectListItem);
            }
            IEnumerable<SelectListItem> myEnum = selectListItemList;
            return myEnum;
        }

        private IEnumerable<SelectListItem> createRoleList()
        {

            List<SelectListItem> myRoleList = new List<SelectListItem>();
            SelectListItem LeadDev = new SelectListItem() { Text = "Lead Dev", Value = "leadDev", Selected = false };
            SelectListItem SeniorDev = new SelectListItem() { Text = "Senior Dev", Value = "seniorDev", Selected = false };
            SelectListItem IntermediateDev = new SelectListItem() { Text = "Intermediate Dev", Value = "intermediateDev", Selected = false };
            SelectListItem JuniorDev = new SelectListItem() { Text = "Junior Dev", Value = "juniorDev", Selected = false };
            SelectListItem LeadQA = new SelectListItem() { Text = "Lead QA", Value = "leadQA", Selected = false };
            SelectListItem SeniorQA = new SelectListItem() { Text = "Senior QA", Value = "seniorQA", Selected = false };
            SelectListItem IntermediateQA = new SelectListItem() { Text = "Intermediate QA", Value = "intermediateQA", Selected = false };
            SelectListItem JuniorQA = new SelectListItem() { Text = "Junior QA", Value = "juniorQA", Selected = false };

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





