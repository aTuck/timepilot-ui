﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Net;
using TimePilot.Models;
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
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string base64Credentials = getEncodedCredentials();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);
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

        public List<Project> parseProjectData(string jsonString)
        {
            List<Project> projects = new List<Project>();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

            for (int i = 0; i < data.Count; i++)
            {
                Project project = new Project();
                dynamic item = data[i];
                project.Key = (string)item.key;
                project.Name = (string)item.name;
                projects.Add(project);
            }
            return projects;
        }
    }
}
