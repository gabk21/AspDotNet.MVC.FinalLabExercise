﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkSchedule.Data;
using WorkSchedule.Data.Entities;
using WorkSchedule.Web.Services;

namespace WorkSchedule.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService employeeService;
        private readonly IUnitOfWork unitOfWork;

        public EmployeeController(IEmployeeService employeeService, IUnitOfWork unitOfWork)
        {
            this.employeeService = employeeService;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("employees")]
        public IActionResult Index()
        {
            var employeeList = employeeService.GetEmployeePage(1, 10);
            return View(employeeList);
        }

        [HttpPost]
        [Route("employees")]
        public IActionResult Index(int currentPageIndex, int pageSize)
        {
            var employeeList = employeeService.GetEmployeePage(currentPageIndex, pageSize);
            return View(employeeList);
        }

        [Route("employees/new")]
        public IActionResult New()
        {
            ViewData["Action"] = "New";
            return View("Form", new Employee());
        }

        [Route("employees/edit")]
        public IActionResult Edit(int id)
        {
            ViewData["Action"] = "Edit";
            var employee = this.unitOfWork.EmployeeRepository.FindByPrimaryKey(id);
            ViewBag.Skills = this.unitOfWork.EmployeeSkillsRepository.GetEmployeeSkillsByEmployeeId(id);
            ViewBag.SkillList = this.unitOfWork.SkillsRepository.FindAll();
            return View("Form", employee);
        }
        public async Task<IActionResult> Save(string action, Employee employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (action.ToLower().Equals("new"))
                    {
                        this.unitOfWork.EmployeeRepository.Insert(employee);
                        await this.unitOfWork.CommitAsync();
                    }
                    if (action.ToLower().Equals("edit"))
                    {
                        this.unitOfWork.EmployeeRepository.Update(employee);
                        await this.unitOfWork.CommitAsync();
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["Action"] = action;
                    if (action.ToLower().Equals("edit"))
                    {
                        ViewBag.Skills = this.unitOfWork.EmployeeSkillsRepository.GetEmployeeSkillsByEmployeeId(employee.ID);
                        ViewBag.SkillList = this.unitOfWork.SkillsRepository.FindAll();
                    }
                    return View("Form", employee);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Form", employee);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = this.unitOfWork.EmployeeRepository.SoftDelete(id);
            await this.unitOfWork.CommitAsync();
            return RedirectToAction("Index");
        }

        [Route("employees/AddSkill")]
        public async Task<IActionResult> AddSkill(int EmployeeId, int SkillId)
        {
            var exists = this.unitOfWork.EmployeeSkillsRepository.checkExistingSkill(EmployeeId, SkillId);
            if (!exists)
            {
                var newSkill = new EmployeeSkill
                {
                    EmployeeID = EmployeeId,
                    SkillID = SkillId
                };
                this.unitOfWork.EmployeeSkillsRepository.Insert(newSkill);
                await this.unitOfWork.CommitAsync();
            }
            else
            {
                ViewBag.Error = "Skill Already Exists";
                ViewData["Action"] = "Edit";
                var employee = this.unitOfWork.EmployeeRepository.FindByPrimaryKey(EmployeeId);
                ViewBag.Skills = this.unitOfWork.EmployeeSkillsRepository.GetEmployeeSkillsByEmployeeId(EmployeeId);
                ViewBag.SkillList = this.unitOfWork.SkillsRepository.FindAll();
                return View("Form", employee);
            }

            return Redirect("Edit?Id=" + EmployeeId);
        }

        //public async Task<IActionResult> SaveSkill(int employeeId, int skillId, string skillName)
        //{
        //    var employee = this.unitOfWork.EmployeeRepository.FindByPrimaryKey(employeeId);
        //    if (this.unitOfWork.SkillsRepository.FindByPrimaryKey(skillId) == null)
        //    {
        //        var newSkill = new Skill()
        //        {
        //            ID = skillId,
        //            Description = skillName
        //        };
        //        this.unitOfWork.SkillsRepository.Insert(newSkill);
        //        await this.unitOfWork.CommitAsync();

        //        var employeeSkill = new EmployeeSkill()
        //        {
        //            ID = (employee.EmployeeSkills.Count + 1),
        //            EmployeeID = employeeId,
        //            Skills = newSkill
        //        };
        //        this.unitOfWork.EmployeeSkillsRepository.Insert(employeeSkill);
        //        await this.unitOfWork.CommitAsync();
        //    }
        //    else
        //    {
        //        ViewData["Action"] = "Invalid input: Skill ID already exist.";
        //        return View("Form", employee);
        //    }
        //    return View("Form", employee);
        //}

        //public async Task<IActionResult> RemoveSkill(int id)
        //{
        //    var employeeSkill = this.unitOfWork.EmployeeSkillsRepository.Delete(id);
        //    await this.unitOfWork.CommitAsync();
        //    return RedirectToAction("Index");
        //}
    }
}
