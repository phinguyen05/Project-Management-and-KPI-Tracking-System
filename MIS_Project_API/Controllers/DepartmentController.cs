﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;
using System.Linq;

namespace MIS_Project_API.Controllers
{
    [Route("api/departments")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public DepartmentController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // GET: api/departments
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetAllDepartments()
        {
            // FE: Departments table expects fields: id, name, managerId
            var departments = await _context.Departments
                .Select(d => new
                {
                    id = d.DepartmentId,
                    name = d.Name,
                    managerId = d.ManagerId
                })
                .ToListAsync();

            return Ok(departments);
        }

        // POST: api/departments
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required.");
            }

            var entity = new Department
            {
                Name = request.Name.Trim(),
                ManagerId = request.ManagerId
            };

            _context.Departments.Add(entity);
            await _context.SaveChangesAsync();

            return Created($"/api/departments/{entity.DepartmentId}", new
            {
                id = entity.DepartmentId,
                name = entity.Name,
                managerId = entity.ManagerId
            });
        }

        // PUT: api/departments/{id}
        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required.");
            }

            var entity = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = request.Name.Trim();
            entity.ManagerId = request.ManagerId;

            _context.Departments.Update(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = entity.DepartmentId,
                name = entity.Name,
                managerId = entity.ManagerId
            });
        }

        // DELETE: api/departments/{id}
        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> DeleteDepartment(int id)
        {
            var entity = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }

        public class CreateDepartmentRequest
        {
            public string Name { get; set; } = null!;
            public int? ManagerId { get; set; }
        }

        public class UpdateDepartmentRequest
        {
            public string Name { get; set; } = null!;
            public int? ManagerId { get; set; }
        }
    }
}
