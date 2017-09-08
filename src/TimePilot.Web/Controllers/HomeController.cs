using System;
using System.Linq;
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
        private string activeSprintJson;

        public static float totalDevCapacity;
        public static int totalStoryPoints;
        public static int totalNumOfSprints;
        public static bool isDoneSortingToBuckets = true;
        public static bool isDoneSaving = true;

        public static int[] StoryPointAllocation;
        private static string SelectedProject;
        public static string SelectedEpic;
        private static string ActiveSprint;

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
        EpicRepository EpicDB = new EpicRepository();

        public void receiveProjectData()
        {
            projectJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            storyJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(story,improvement)%20and%20%28status%20not%20in(closed,done,resolved,accepted)+OR+Sprint+IN+openSprints%28%29%29&fields=customfield_10013,customfield_10830,id,description,summary&maxResults=1000");
        }

        public void recieveEpicData()
        {
            epicJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "%20AND%20type%20in(epic)%20and%20(Sprint=EMPTY%20OR%20Sprint%20not%20in(openSprints(),futureSprints()))%20AND%20status%20not%20in(closed,done,resolved,accepted)&fields=customfield_10013,customfield_10830,id,description,summary&maxResults=1000");
        }

        public void receiveActiveSprintData()
        {
            activeSprintJson = apiHelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/search?jql=project=" + "'" + SelectedProject + "'" + "+AND+issuetype+%3D+Story+AND+Sprint+IN+openSprints%28%29+ORDER+BY+priority+DESC%2C+updated+DESC&fields=customfield_10331");
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
                temp = ProjDB.GetById(projects[i].ProjectKey);
                if (!(temp.ProjectKey == projects[i].ProjectKey))
                    ProjDB.Add(projects[i]);
                }
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 0, VaryByParam = "none", NoStore = true)]
        public ActionResult Story()
        {
            System.Threading.Thread.Sleep(350);
            while (!isDoneSaving)
            {
                System.Threading.Thread.Sleep(100);
            }
            isDoneSortingToBuckets = false;

            stories = StoryDB.GetAllByForeignId(SelectedProject);
            epics = EpicDB.GetAllByForeignId(SelectedProject);

            StoryVM.EpicList = new Dictionary<string, Epic>();
            StoryVM.StoryList = stories;
            for (int i = 0; i < epics.Count; i++) {
                if (checkForEpicInStories(epics[i].EpicKey))
                {
                    StoryVM.EpicList.Add(epics[i].EpicKey, epics[i]);
                }
            }
            sortStoryPointsIntoBuckets();

            receiveActiveSprintData();
            StoryVM.StoriesInActiveSprintList = apiHelper.parseActiveSprintData(activeSprintJson);
            StoryVM.activeSprint = parseActiveSprintString(apiHelper.getActiveSprint(activeSprintJson));
            StoryVM.selectedProject = ProjDB.GetById(SelectedProject).Summary;
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
            return RedirectToAction("EpicPopulate");
        }

        public ActionResult EpicPopulate()
        {
            recieveEpicData();
            epics = apiHelper.parseEpicData(epicJson);
            Epic temp = new Epic();
            for (int i = 0; i < epics.Count; i++)
            {
                // TODO: Bring this logic into repository
                epics[i].ProjectKey = SelectedProject;
                temp = EpicDB.GetById(epics[i]);
                if (temp.ProjectKey == null)
                    EpicDB.Add(epics[i]);
            }
            return RedirectToAction("Story");
        }

        [HttpPost]
        public ActionResult StoryUpdate(StoryViewModel m)
        {
            isDoneSaving = false;
            List<Story> storiesToBeSorted = new List<Story>();
            StoryVM = m;
            if (m.StoryList != null)
            {
                foreach (var s in m.StoryList)
                {
                    s.ProjectKey = SelectedProject;
                    StoryDB.Update(s);

                    if (s.isHidden == 0)
                    {
                        storiesToBeSorted.Add(s);
                    }
                }
            }
            totalStoryPoints = m.totalStoryPoints;
            StoryVM.StoryList = storiesToBeSorted;
            sortStoryPointsIntoBuckets();
            isDoneSortingToBuckets = true;
            isDoneSaving = true;
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
            System.Threading.Thread.Sleep(350);
            while (!isDoneSaving)
            {
                System.Threading.Thread.Sleep(100);
            }
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
            isDoneSaving = false;
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

            isDoneSaving = true;
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
            // Bug:
            // When proceeding through resource page too quick, sorting
            // the correct stories into buckets isn't complete yet and results
            // page uses incorrect stories.
            // Fix: Spin until it's done
            System.Threading.Thread.Sleep(350);
            while (!isDoneSortingToBuckets || !isDoneSaving)
            {
                System.Threading.Thread.Sleep(100);
            }
            ResultsVM.storyPointAllocation = StoryPointAllocation;
            ResultsVM.DaysPerPt = ConversionRateDB.GetAllByForeignId(SelectedProject);
            
            // For new projects - initialize conversion rates
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
            isDoneSaving = false;
            List<int> listOfIDs = new List<int>();
            for (int i = 0; i<m.DaysPerPt.Count; i++)
            {
                m.DaysPerPt[i].ProjectKey = SelectedProject;
                listOfIDs.Add(ConversionRateDB.Add(m.DaysPerPt[i]));
            }
            isDoneSaving = true;
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

        private bool checkForEpicInStories(string key)
        {
            for (int i = 0; i < StoryVM.StoryList.Count; i++)
            {
                if (StoryVM.StoryList[i].EpicKey == key)
                {
                    return true;
                }
            }
            return false;
        }

        private Conversion generateConversion(int n)
        {
            Conversion c = new Conversion();
            c.StoryPoints = n;
            return c;
        }

        private string parseActiveSprintString(string s)
        {
            if (s != "")
            {
                int index1 = s.IndexOf("name=")+5;
                int index2 = s.IndexOf(",goal=");
                s = s.Substring(index1, index2-index1).Trim();
                return s;
            }
            else
            {
                return "No Active Sprint";
            }
        }

    }
}





