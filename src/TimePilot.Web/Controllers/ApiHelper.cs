using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Net;
using TimePilot.Web.Models;
using TimePilot.Entities.Project;
using Newtonsoft.Json.Linq;

namespace TimePilot.Controllers
{
    public class ApiHelper
    {
        List<TimePilot.Entities.Project.Project> projects = new List<TimePilot.Entities.Project.Project>();

        private const string USERNAME = "developer";
        private const string PASSWORD = "PN!M3d!a";

        public string getDataFromJira(string Url)
        {
            string responseInString = "";
            HttpClient client = new HttpClient();
            string base64Credentials = getEncodedCredentials();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(Url).Result;
            responseInString = response.Content.ReadAsStringAsync().Result;
            return responseInString;
        }

        private string getEncodedCredentials()
        {
            string mergedCredentials = string.Format("{0}:{1}", USERNAME, PASSWORD);
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }

        public List<Story> parseStoryData(string jsonString)
        {
            List<Story> stories = new List<Story>();
            JObject results = JObject.Parse(jsonString);

            foreach (var result in results["issues"])
            {
                Story story = new Story();

                story.Key = (string)result["key"];
                story.Summary = (string)result["fields"]["summary"];
                story.Description = (string)result["fields"]["description"];
                story.StoryPoint = (string)result["fields"]["customfield_10013"];

                stories.Add(story);
            }
            return stories;
        }

        public List<TimePilot.Entities.Project.Project> parseProjectData(string jsonString)
        {
            List<TimePilot.Entities.Project.Project> projects = new List<TimePilot.Entities.Project.Project>();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

            for (int i = 0; i < data.Count; i++)
            {
                TimePilot.Entities.Project.Project project = new TimePilot.Entities.Project.Project();
                dynamic item = data[i];
                project.ProjectKey = (string)item.key;
                project.Summary = (string)item.name;
                projects.Add(project);
            }
            return projects;
        }
    }
}
