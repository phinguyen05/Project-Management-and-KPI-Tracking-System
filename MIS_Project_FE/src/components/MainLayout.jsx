import React, { useState, useEffect } from 'react';
import { Layout, Menu, Button, Dropdown, Avatar, theme, message, Badge, List, Card } from 'antd';
import {
    DashboardOutlined, ProjectOutlined, UserOutlined, SettingOutlined,
    FileTextOutlined, LogoutOutlined, CheckSquareOutlined,
    MenuFoldOutlined, MenuUnfoldOutlined, TeamOutlined, BellOutlined
} from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import api from '../services/api';

const { Header, Sider, Content } = Layout;

export default function MainLayout({ children }) {
    const [collapsed, setCollapsed] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const { token: { colorBgContainer, borderRadiusLG } } = theme.useToken();
    const navigate = useNavigate();
    const location = useLocation();

    const roleRaw = localStorage.getItem('role') || 'Employee';
    const normalizeRoleValue = (r) => {
        const s = (r ?? '').toString().trim();
        const upper = s.replace(/_/g, '-').replace(/\s+/g, '-').toUpperCase();
        if (upper === 'ADMIN') return 'Admin';
        if (upper === 'MANAGER') return 'Manager';
        if (upper === 'EMPLOYEE') return 'Employee';
        if (upper === 'CLEVEL' || upper === 'C-LEVEL' || upper === 'C_LEVEL') return 'C-Level';
        return s;
    };

    const role = normalizeRoleValue(roleRaw);

    // Lấy thêm username/fullName từ localStorage (fallback an toàn)
    const usernameFromStorage = localStorage.getItem('username') || localStorage.getItem('fullName');
    const username = usernameFromStorage ? usernameFromStorage.toString().trim() : '';

    // Hiển thị: {username} - {role} (ví dụ: nguyenvana - C-Level)
    // Nếu không lấy được username -> hiển thị role như cũ.
    const displayUserName = username ? `${username} - ${role}` : role;

    // Tải thông báo từ Backend (không dùng mock data)
    const fetchNotifications = async () => {
        try {
            const res = await api.get('/notifications');
            setNotifications(Array.isArray(res.data) ? res.data : []);
        } catch (err) {
            setNotifications([]);
        }
    };

    useEffect(() => {
        fetchNotifications();
        const intervalId = setInterval(() => {
            fetchNotifications();
        }, 30000);

        return () => clearInterval(intervalId);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const markAsRead = async (id) => {
        try {
            await api.put(`/notifications/${id}/read`);
            message.success('Đã đánh dấu đã đọc');
            fetchNotifications();
        } catch (err) {
            message.error('Không thể đánh dấu đã đọc');
        }
    };


    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('fullName');
        localStorage.removeItem('username');
        message.success('Đã đăng xuất!');
        navigate('/login');
    };

    const getMenuItems = () => {
        // BẮT BUỘC: role đã được normalize ở trên
        if (role === 'Admin') {
            return [
                { key: '/admin', icon: <UserOutlined />, label: 'Tài khoản' },
                { key: '/admin/departments', icon: <TeamOutlined />, label: 'Phòng ban' },
                { key: '/admin/settings', icon: <SettingOutlined />, label: 'Cấu hình hệ thống' },
                { key: '/admin/audit-log', icon: <FileTextOutlined />, label: 'Audit Log' },
            ];
        }

        if (role === 'Manager') {
            return [
                { key: '/manager', icon: <DashboardOutlined />, label: 'Dashboard' },
                { key: '/manager/projects', icon: <ProjectOutlined />, label: 'Quản lý Dự án' },
                { key: '/manager/approvals', icon: <CheckSquareOutlined />, label: 'Phê duyệt' },
                { key: '/manager/kpi-review', icon: <SettingOutlined />, label: 'Chốt KPI Cuối Tháng' },
            ];
        }

        if (role === 'C-Level') {
            // C-Level chỉ được 1 menu: Tổng quan Dự án
            return [
                { key: '/c-level', icon: <DashboardOutlined />, label: 'Tổng quan Dự án' },
            ];
        }

        // Employee
        return [
            { key: '/employee', icon: <DashboardOutlined />, label: 'Trang chủ' },
            { key: '/employee/tasks', icon: <ProjectOutlined />, label: 'Công việc' },
            { key: '/employee/log-time', icon: <FileTextOutlined />, label: 'Lịch sử Log-time' },
            { key: '/employee/leave', icon: <CheckSquareOutlined />, label: 'Xin nghỉ phép' },
        ];
    };

    const userMenu = {
        items: [{ key: 'logout', icon: <LogoutOutlined />, label: 'Đăng xuất', onClick: handleLogout }]
    };

    // Nội dung danh sách thông báo thả xuống
    const notificationMenu = (
        <Card style={{ width: 350, maxHeight: 400, overflowY: 'auto' }} title="Bảng Thông Báo" size="small">
            <List
                size="small"
                dataSource={notifications}
                renderItem={item => (
                    <List.Item
                        style={{ cursor: 'pointer', background: item.isRead ? '#ffffff' : '#e6f7ff', padding: '8px 12px', borderBottom: '1px solid #f0f0f0' }}
                        onClick={() => handleNotificationClick(item)}
                    >
                        <List.Item.Meta
                            title={<span style={{ fontWeight: item.isRead ? 'normal' : 'bold', fontSize: '13px' }}>{item.message}</span>}
                            description={<span style={{ fontSize: '11px' }}>{item.createdAt ? new Date(item.createdAt).toLocaleDateString() : ''}</span>}
                        />
                    </List.Item>
                )}
            />
        </Card>
    );

    const handleNotificationClick = async (item) => {
        if (!item?.isRead) {
            // UX: cập nhật UI ngay lập tức
            setNotifications((prev) =>
                prev.map((n) => (n.id === item.id ? { ...n, isRead: true } : n))
            );

            try {
                await api.put(`/notifications/${item.id}/read`);
            } catch (err) {
                // Nếu call API thất bại thì revert
                setNotifications((prev) =>
                    prev.map((n) => (n.id === item.id ? { ...n, isRead: false } : n))
                );
                message.error('Không thể đánh dấu đã đọc');
                return;
            }

            message.success('Đã đánh dấu đã đọc');
            return;
        }

        message.info('Thông báo này đã được đọc');
    };


    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sider trigger={null} collapsible collapsed={collapsed} theme="light">
                <div style={{ height: 64, margin: 16, background: 'rgba(0, 0, 0, 0.05)', borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold' }}>
                    {collapsed ? 'MIS' : 'KPI SYSTEM'}
                </div>
                <Menu theme="light" mode="inline" selectedKeys={[location.pathname]} items={getMenuItems()} onClick={({ key }) => navigate(key)} />
            </Sider>
            <Layout>
                <Header style={{ padding: '0 24px', background: colorBgContainer, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Button type="text" icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />} onClick={() => setCollapsed(!collapsed)} />
                    <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
                        <Dropdown popupRender={() => notificationMenu} trigger={['click']}>
                            <Badge count={notifications.filter((n) => !n?.isRead).length} size="small">
                                <BellOutlined style={{ fontSize: '20px', cursor: 'pointer' }} />
                            </Badge>
                        </Dropdown>
                        <Dropdown menu={userMenu} placement="bottomRight">
                            <span style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8 }}>
                                <Avatar icon={<UserOutlined />} />
                                <span>{displayUserName}</span>
                            </span>
                        </Dropdown>
                    </div>
                </Header>
                <Content style={{ margin: '24px 16px', padding: 24, background: colorBgContainer, borderRadius: borderRadiusLG }}>
                    {children}
                </Content>
            </Layout>
        </Layout>
    );
}

