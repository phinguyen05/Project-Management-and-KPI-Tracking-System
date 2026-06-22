import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import React from 'react';
import Login from './pages/Login';
import MainLayout from './components/MainLayout';
import UserManagement from './pages/admin/UserManagement';
import DepartmentManagement from './pages/admin/DepartmentManagement';
import SystemSettings from './pages/admin/SystemSettings';
import AuditLogPage from './pages/admin/AuditLogPage';
import ProjectList from './pages/manager/ProjectList';
import ProjectManagement from './pages/manager/ProjectManagement';
import Approvals from './pages/manager/Approvals';

import KanbanBoard from './pages/employee/KanbanBoard';
import EmployeeDashboard from './pages/employee/EmployeeDashboard';
import ManagerDashboard from './pages/manager/ManagerDashboard';
import CLevelDashboard from './pages/clevel/CLevelDashboard';
import ApprovalCenter from './pages/manager/ApprovalCenter';
import KPIReview from './pages/manager/KPIReview';
import LogTimeHistory from './pages/employee/LogTimeHistory';

// Helper component để kiểm tra Auth và phân quyền (Route Guard)
function RequireAuth({ children, allowedRoles }) {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !allowedRoles.includes(role)) {
    // Nếu role không hợp lệ, redirect về trang chủ của role đó
    return <Navigate to={getHomePathByRole(role)} replace />;
  }

  return children;
}

function getHomePathByRole(role) {
  if (role === 'Admin') return '/admin';
  if (role === 'Manager') return '/manager';
  if (role === 'Employee') return '/employee';
  if (role === 'C-Level') return '/c-level';
  return '/login';
}

function App() {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        {/* Admin Routes */}
        <Route 
          path="/admin" 
          element={
            <RequireAuth allowedRoles={["Admin"]}>
              <MainLayout><UserManagement /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/admin/departments" 
          element={
            <RequireAuth allowedRoles={["Admin"]}>
              <MainLayout><DepartmentManagement /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/admin/settings" 
          element={
            <RequireAuth allowedRoles={["Admin"]}>
              <MainLayout><SystemSettings /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/admin/audit-log" 
          element={
            <RequireAuth allowedRoles={["Admin"]}>
              <MainLayout><AuditLogPage /></MainLayout>
            </RequireAuth>
          } 
        />

        {/* Manager Routes */}
        <Route 
          path="/manager" 
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout><ManagerDashboard /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route path="/manager/kpi-review" element={<MainLayout><KPIReview /></MainLayout>} />
        <Route 
          path="/manager/projects" 
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout><ProjectList /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/manager/projects/:project_id/tasks" 
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout><ProjectManagement /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route path="/manager/approvals" element={<MainLayout><ApprovalCenter /></MainLayout>} />
        <Route 
          path="/manager/approvals-list" 
          element={
            <RequireAuth allowedRoles={['Manager']}>
              <MainLayout><Approvals /></MainLayout>
            </RequireAuth>
          } 
        />

        {/* Employee Routes */}
        <Route 
          path="/employee" 
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout><EmployeeDashboard /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/employee/tasks" 
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout><KanbanBoard /></MainLayout>
            </RequireAuth>
          } 
        />
        <Route 
          path="/employee/log-time" 
          element={
            <RequireAuth allowedRoles={['Employee']}>
              <MainLayout><LogTimeHistory /></MainLayout>
            </RequireAuth>
          } 
        />

        {/* C-Level Routes */}
        <Route 
          path="/c-level" 
          element={
            <RequireAuth allowedRoles={['C-Level']}>
              <MainLayout><CLevelDashboard /></MainLayout>
            </RequireAuth>
          } 
        />

        {/* Fallback Route */}
        <Route path="*" element={<Navigate to={token ? getHomePathByRole(role) : "/login"} replace />} />
      </Routes>
    </Router>
  );
}

export default App;
