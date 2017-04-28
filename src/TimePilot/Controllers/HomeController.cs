using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Models;


namespace TimePilot.Controllers
{
    public class HomeController : Controller
    {

        ApiHelper myhelper = new ApiHelper();
        private string projectJson;
        private string storyJson;
        List<Project> projects = new List<Project>();

        public string SelectedProject;

        public void receiveProjectData()
        {
            this.projectJson = myhelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/project/");
        }

        public void receiveStoryData()
        {
            this.storyJson = myhelper.getDataFromJira("https://pnimedia.jira.com/rest/api/2/APOLLO?_=1492122231729");
        }


        public ActionResult Index()
        {

            receiveProjectData();
            projects = myhelper.parseProjectData(projectJson);


            ViewBag.ListItems = convertProjectListToIEnum(projects);
            return View();

        }

        [HttpPost]
        public ActionResult Index(Project project)
        {
            receiveProjectData();
            projects = myhelper.parseProjectData(projectJson);

            ViewBag.ListItems = convertProjectListToIEnum(projects);

            ViewData["myProject"] = Request["ddlProjects"];
            this.SelectedProject = Request["ddlProjects"];
            ViewData["myThis"] = SelectedProject;
            return View();
        }

        public IEnumerable<SelectListItem> convertProjectListToIEnum(List<Project> projectList)
        {
            List<SelectListItem> listSelectListItem = new List<SelectListItem>();

            for (int i = 0; i < projectList.Count; i++)
            {

                SelectListItem selectListItem = new SelectListItem()
                {
                    Text = projectList[i].Name + "        " + projectList[i].Key,
                    Value = projectList[i].Key,
                    Selected = false
                };


                listSelectListItem.Add(selectListItem);

            }

            IEnumerable<SelectListItem> myEnum = listSelectListItem;
            return myEnum;
        }

        public ActionResult Stories()
        {

            receiveProjectData();
            projects = myhelper.parseProjectData(projectJson);


            ViewBag.ListItems = convertProjectListToIEnum(projects);
            return View();

        }

    }
}


