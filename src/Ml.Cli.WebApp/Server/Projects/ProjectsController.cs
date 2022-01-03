using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ml.Cli.WebApp.Server.Projects
{
    [Route("api/server/[controller]")]
    [ApiController]
    public class ProjectsController : Controller
    {
        private static List<Project> projects;

        private Project find(string id)
        {
            return projects.Find(currentProject => currentProject.Id.Equals(id));
        }

        public ProjectsController()
        {
            if (projects == null)
            {
                Console.WriteLine("Loading projects...");
                string projectsAsString = System.IO.File.ReadAllText("./Server/Projects/mocks/projects.json");
                var projectsAsJsonnFile = JsonDocument.Parse(projectsAsString);
                var projectsAsJson = projectsAsJsonnFile.RootElement.GetProperty("projects");
                projects = JsonConvert.DeserializeObject<List<Project>>(projectsAsJson.ToString());
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<Project>> GetAllProjects()
        {
            return Ok(projects);
        }

        [HttpGet("{id}", Name = "GetProjectById")]
        public ActionResult<Project> GetProject(string id)
        {
            var project = find(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Project> Create(Project newProject)
        {
            newProject.Id = Guid.NewGuid().ToString();
            newProject.CreateDate = DateTime.Now.ToString("dd/MM/yyyy");
            projects.Add(newProject);
            
            return find(newProject.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult<Project> Delete(string id)
        {
            var project = find(id);
            if (project == null)
            {
                return NotFound();
            }

            projects.Remove(project);
            return this.NoContent();
        }
    }
}