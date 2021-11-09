using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BabAl_SalamWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;

namespace BabAl_SalamWebAPI.Controllers
{
    [EnableCors("AllowReactFEBabAl-Salam")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ProjectsController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProject()
        {
            //return await _context.Project.ToListAsync();
            return await _context.Project
                .Select(x => ProjectToDTO(x))
                .ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectInformation>> GetProject(long id)
        {
            var project = await _context.Project.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            var projectInfo = new ProjectInformation
            {
                Project = ProjectToDTO(project)
            };

            return projectInfo;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseResult>> PutProject(long id, ProjectDTO projectDTO)
        {
            ResponseResult responseObj;

            if (id != projectDTO.Id)
            {
                return BadRequest();
            }

            var project = await _context.Project.FindAsync(id);

            if(project == null)
            {
                return NotFound();
            }

            var findProjectByTitle = _context.Project.Any(project =>
                    project.Title.ToLower() == projectDTO.Title.ToLower()
                );

            if(findProjectByTitle) {
                return (
                    responseObj = new ResponseResult
                    {
                        ResponseMessage = $"{projectDTO.Title} already exists.",
                        MessageStanding = "red",
                    }
                );
            }

            project.Title = projectDTO.Title;
            project.Description = projectDTO.Description;

            var success = await _context.SaveChangesAsync() > 0;

            if(success)
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Successfully edited {project.Title}.",
                    MessageStanding = "green",
                    Data = new ProjectDataInformation
                    {
                        Projects = GetProject().Result.Value
                    }
                };
            } else
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"An issue has occured while editing {project.Title}. Please try again later.",
                    MessageStanding = "red",
                };
            }

            return responseObj;
        }

        // POST: api/Projects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseResult>> PostProject(ProjectDTO projectDTO)
        {
            ResponseResult responseObj;

            var findProjectByTitle = _context.Project.Any(project =>
                    project.Title.ToLower() == projectDTO.Title.ToLower()
                );

            if (findProjectByTitle)
            {
                return (
                    responseObj = new ResponseResult
                    {
                        ResponseMessage = $"{projectDTO.Title} already exists.",
                        MessageStanding = "red",
                    }
                );
            }

            var project = new Project
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description
            };

            _context.Project.Add(project);

            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Successfully created {project.Title}.",
                    MessageStanding = "green",
                    Data = new ProjectDataInformation
                    {
                        Projects = GetProject().Result.Value
                    }
                };
            }
            else
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"An issue has occured while creating {project.Title}. Please try again later.",
                    MessageStanding = "red",
                };
            }

            return responseObj;

        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResponseResult> DeleteProject(long id)
        {
            ResponseResult responseObj;

            var project = await _context.Project.FindAsync(id);

            if (project == null)
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Project Not Found",
                    MessageStanding = "red",
                };
            }

            _context.Project.Remove(project);
            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Successfully deleted {project.Title}.",
                    MessageStanding = "green",
                    Data = new ProjectDataInformation
                    {
                        Projects = GetProject().Result.Value
                    }
                };
            }
            else
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"An issue has occured while deleting {project.Title}. Please try again later.",
                    MessageStanding = "red",
                };
            }

            return responseObj;
        }

        private static ProjectDTO ProjectToDTO(Project project) =>
            new ProjectDTO
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description
            };

    }
}
