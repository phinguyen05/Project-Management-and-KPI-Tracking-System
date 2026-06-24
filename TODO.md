# TODO

## Backend Department CRUD + Route Sync
- [x] Update `MIS_Project_API/Controllers/DepartmentController.cs`
  - [x] Change controller route to `[Route("api/departments")]`
  - [x] Ensure `[HttpGet]` returns fields compatible with Frontend
  - [x] Add `[HttpPost]` create department
  - [x] Add `[HttpPut("{id}")]` update department by id
  - [x] Add `[HttpDelete("{id}")]` delete department by id
  - [x] Implement using `_context.Departments` + `await _context.SaveChangesAsync()`
- [x] Run build/test (dotnet) để đảm bảo không lỗi cú pháp


