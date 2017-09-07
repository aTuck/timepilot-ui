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
using TimePilot.Entities;
using Newtonsoft.Json.Linq;

namespace TimePilot.Controllers
{
    public class ApiHelper
    {
        List<Project> projects = new List<Project>();

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


            if (jsonString.Contains("issues"))
            {
                foreach (var result in results["issues"])
                {
                    Story story = new Story();

                    story.StoryID = (int)result["id"];
                    story.StoryKey = (string)result["key"];
                    story.EpicKey = (string)result["fields"]["customfield_10830"];
                    story.Summary = (string)result["fields"]["summary"];
                    story.StoryPoints = (int?)result["fields"]["customfield_10013"] ?? 0;

                    stories.Add(story);
                }
            }

            return stories;
        }

        public List<Epic> parseEpicData(string jsonString)
        {
            List<Epic> epics = new List<Epic>();
            JObject results = JObject.Parse(jsonString);
            if (jsonString.Contains("issues"))
            {
                foreach (var result in results["issues"])
                {
                    Epic epic = new Epic();
                    epic.EpicKey = (string)result["key"];
                    epic.Summary = (string)result["fields"]["summary"];
                    epics.Add(epic);
                }
            }
            return epics;
        }

        public List<Project> parseProjectData(string jsonString)
        {
            List<Project> projects = new List<Project>();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

            for (int i = 0; i < data.Count; i++)
            {
                Project project = new Project();
                dynamic item = data[i];
                project.ProjectKey = (string)item.key;
                project.Summary = (string)item.name;
                projects.Add(project);
            }
            return projects;
        }

        public List<string> parseActiveSprintData(string jsonString)
        {
            List<string> StoriesInActiveSprintList = new List<string>();
            JObject results = JObject.Parse(jsonString);

            if (jsonString.Contains("issues"))
            {
                foreach (var result in results["issues"])
                {
                    StoriesInActiveSprintList.Add((string)result["key"]);
                }
            }

            return StoriesInActiveSprintList;
        }

        public string getActiveSprint(string jsonString)
        {
            string activeSprint = "";
            JObject results = JObject.Parse(jsonString);

            if (jsonString.Contains("fields"))
            {
                activeSprint = (string)results["issues"][0]["fields"]["customfield_10331"][0];
            }

            return activeSprint;
        }
    }
}
