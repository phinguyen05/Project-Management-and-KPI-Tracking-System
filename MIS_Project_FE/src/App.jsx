import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import React from 'react';
import Login from './pages/Login';
import MainLayout from './components/MainLayout';
import Unauthorized from './pages/Unauthorized';

import UserManagement from './pages/admin/UserManagement';
import DepartmentManagement from './pages/admin/DepartmentManagement';
import SystemSettings from './pages/admin/SystemSettings';
import AuditLogPage from './pages/admin/AuditLogPage';

import ProjectList from './pages/manager/ProjectList';
import ProjectManagement from './pages/manager/ProjectManagement';
import Approvals from './pages/manager/Approvals';
import KPIReview from './pages/manager/KPIReview';
import ApprovalCenter from './pages/manager/ApprovalCenter';

import KanbanBoard from './pages/employee/KanbanBoard';
import EmployeeDashboard from './pages/employee/EmployeeDashboard';
import LogTimeHistory from './pages/employee/LogTimeHistory';
import LeaveRequest from './pages/employee/LeaveRequest';

import ManagerDashboard from './pages/manager/ManagerDashboard';
import CLevelDashboard from './pages/clevel/CLevelDashboard';

function normalizeRoleValue(r) {
  const s = (r ?? '').toString().trim();
  const upper = s.replace(/_/g, '-').replace(/\s+/g, '-').toUpperCase();

  if (upper === 'ADMIN') return 'Admin';
  if (upper === 'MANAGER') return 'Manager';
  if (upper === 'EMPLOYEE') return 'Employee';

  // Canonical: always use 'C-Level'
  if (upper === 'CLEVEL' || upper === 'C-LEVEL' || upper === 'C_LEVEL') return 'C-Level';

  return s ? s : '';
}

function getHomePathByRole(role) {
  const r = normalizeRoleValue(role);

  if (r === 'Admin') return '/admin';
  if (r === 'Manager') return '/manager';
  if (r === 'Employee') return '/employee';

  // BẮT BUỘC: C-Level luôn map về '/c-level'
  if (r === 'C-Level') return '/c-level';

  return '/login';
}

function RequireAuth({ children, allowedRoles }) {
  const token = localStorage.getItem('token');
  const roleRaw = localStorage.getItem('role');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  const role = normalizeRoleValue(roleRaw);
  const allowed = Array.isArray(allowedRoles) ? allowedRoles.map(normalizeRoleValue) : null;

  if (allowed && !allowed.includes(role)) {
    // Không đẩy về home/login để tránh vòng lặp.
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
}


function App() {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />

        {/* Admin */}
        <Route
          path="/admin"
          element={
            <RequireAuth allowedRoles={['Admin']}>
              <MainLayout>
                <UserManagement />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/admin/departments"
          element={
            <RequireAuth allowedRoles={['Admin']}>
              <MainLayout>
                <DepartmentManagement />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/admin/settings"
          element={
            <RequireAuth allowedRoles={['Admin']}>
              <MainLayout>
                <SystemSettings />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/admin/audit-log"
          element={
            <RequireAuth allowedRoles={['Admin']}>
              <MainLayout>
                <AuditLogPage />
              </MainLayout>
            </RequireAuth>
          }
        />

        {/* Manager */}
        <Route
          path="/manager"
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout>
                <ManagerDashboard />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route path="/manager/kpi-review" element={<MainLayout><KPIReview /></MainLayout>} />
        <Route
          path="/manager/projects"
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout>
                <ProjectList />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/manager/projects/:project_id/tasks"
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout>
                <ProjectManagement />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route path="/manager/approvals" element={<MainLayout><ApprovalCenter /></MainLayout>} />
        <Route
          path="/manager/approvals-list"
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout>
                <Approvals />
              </MainLayout>
            </RequireAuth>
          }
        />

        {/* Employee */}
        <Route
          path="/employee"
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout>
                <EmployeeDashboard />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/employee/tasks"
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout>
                <KanbanBoard />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/employee/log-time"
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout>
                <LogTimeHistory />
              </MainLayout>
            </RequireAuth>
          }
        />
        <Route
          path="/employee/leave"
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout>
                <LeaveRequest />
              </MainLayout>
            </RequireAuth>
          }
        />

        {/* C-Level */}
        <Route
          path="/c-level"
          element={
            <RequireAuth allowedRoles={['C-Level', 'C-level']}>
              <MainLayout>
                <CLevelDashboard />
              </MainLayout>
            </RequireAuth>
          }
        />

        {/* Unauthorized */}
        <Route path="/unauthorized" element={<Unauthorized />} />

        {/* Fallback */}
        <Route
          path="*"
          element={<Navigate to={token ? getHomePathByRole(role) : '/login'} replace />}
        />
      </Routes>
    </Router>
  );
}

export default App;

