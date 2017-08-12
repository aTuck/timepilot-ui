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
        private string epicJson;

        public static float totalDevCapacity;
        public static int totalStoryPoints;
        public static int totalNumOfSprints;

        public static int[] StoryPointAllocation;
        private static string SelectedProject = "";
        public static string SelectedEpic;

        List<Story> stories = new List<Story>();        
        List<Project> projects = new List<Project>();
        List<Epic> epics = new List<Epic>();

        ProjectViewModel ProjectVM = new ProjectViewModel();
        StoryViewModel StoryVM = new StoryViewModel();
        ResourceCapacityViewModel ResourceVM = new ResourceCapacityViewModel();
        ResultsViewModel ResultsVM = new ResultsViewModel();

        ProjectRepository ProjDB = new ProjectRepository();
        StoryRepository StoryDB = new StoryRepository();
        SprintRepository SprintDB = new SprintRepository();
        MemberRepository MemberDB = new MemberRepository();
        ConversionRepository ConversionRateDB = new ConversionRepository();

        public void receiveProjectData()
        {
            projectJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            storyJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(story,improvement)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,customfield_10830,id,description,summary&maxResults=1000");
        }

        public void recieveEpicData()
        {
            epicJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(epic)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,customfield_10830,id,description,summary&maxResults=1000");
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
            recieveEpicData();
            epics = apiHelper.parseEpicData(epicJson);
            StoryVM.EpicList = epics;
            sortStoryPointsIntoBuckets();
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


        [OutputCache(Duration = 0, VaryByParam = "none", NoStore = true)]
        public ActionResult Resource()
        {
            List<Member> currentMembers = new List<Member>();
            ResourceVM.members = currentMembers;
            ResourceVM.sprints = SprintDB.GetAllByForeignId(SelectedProject);
            if (ResourceVM.sprints.Count == 0)
            {
                Sprint initSprint = new Sprint();
                initSprint.Name= "Enter Sprint Name...";
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
            totalDevCapacity = m.totalDevCapacity;
            totalNumOfSprints = m.sprints.Count;
            int dbCurrentSprintID = 0;
            int tempCurrentSprintID = 0;
            for (int i=0; i < m.sprints.Count; i++)
            {
                // Grab working reference to current sprint -- this ID is different than database ID
                tempCurrentSprintID = m.sprints[i].PageID;

                // Save Sprint to DB
                m.sprints[i].ProjectKey = SelectedProject;
                if (m.sprints[i].PageID != -1) { dbCurrentSprintID = SprintDB.Add(m.sprints[i]); }

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
            
            return RedirectToAction("Resource");
        }

        [HttpPost]
        public ActionResult ResourceMemberDelete(int[] id)
        {
            foreach (var item in id)
            {
                MemberDB.Delete(item);
            }
            return RedirectToAction("Resource");
        }

        [HttpPost]
        public ActionResult ResourceSprintDelete(int id)
        {
            MemberDB.DeleteAllByForeignID(id);
            SprintDB.Delete(id);
            return RedirectToAction("Resource");
        }

        public ActionResult Result()
        {
            ResultsVM.storyPointAllocation = StoryPointAllocation;
            ResultsVM.DaysPerPt = ConversionRateDB.GetAllByForeignId(SelectedProject);
            
            // New project - initialize conversion rates
            if (ResultsVM.DaysPerPt.Count <= 0 )
            {
                var storyPointTiers = new List<int> { 1, 3, 5, 8, 13, 21 };
                var initDaysPerPt = new List<Conversion>();
                for (int i = 0; i < storyPointTiers.Count; i++) { initDaysPerPt.Add(generateConversion(storyPointTiers[i])); };
                ResultsVM.DaysPerPt = initDaysPerPt;
            }

            ResultsVM.totalDevCapacity = totalDevCapacity;
            ResultsVM.totalStoryPoints = totalStoryPoints;
            ResultsVM.totalNumOfSprints = totalNumOfSprints;

            return View(ResultsVM);
        }

        [HttpPost]
        public void ResultUpdate(ResultsViewModel m)
        {
            List<int> listOfIDs = new List<int>();
            for (int i = 0; i<m.DaysPerPt.Count; i++)
            {
                m.DaysPerPt[i].ProjectKey = SelectedProject;
                listOfIDs.Add(ConversionRateDB.Add(m.DaysPerPt[i]));
            }
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

        private Conversion generateConversion(int n)
        {
            Conversion c = new Conversion();
            c.StoryPoints = n;
            return c;
        }
    }
}





