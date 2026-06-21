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

    const role = localStorage.getItem('role') || 'Employee';
    const fullName = localStorage.getItem('fullName') || role;

    // Tải thông báo từ Backend
    const fetchNotifications = async () => {
        try {
            const res = await api.get('/notification');
            setNotifications(res.data);
        } catch (err) {
            console.log("Chưa có API thông báo hoặc lỗi kết nối.");
            setNotifications([
                { notifId: 1, content: "[Task Mới] Quản lý vừa giao task 'Viết API Backend' cho bạn", isRead: false, createdAt: new Date() },
                { notifId: 2, content: "[Sắp đến hạn] Task 'Fix lỗi UI bảng Gantt' sẽ hết hạn vào ngày mai", isRead: false, createdAt: new Date() },
                { notifId: 3, content: "[Trễ hạn] Task 'Setup DB' đã trễ hạn 1 ngày. Vui lòng cập nhật ngay!", isRead: true, createdAt: new Date() }
            ]);
        }
    };

    useEffect(() => {
        fetchNotifications();
    }, []);

    const markAsRead = async (notifId) => {
        try {
            await api.patch(`/notification/${notifId}/read`);
            message.success("Đã đánh dấu đã đọc");
            fetchNotifications();
        } catch (err) {
            // Giả lập mark as read thành công
            setNotifications(notifications.map(n => n.notifId === notifId ? { ...n, isRead: true } : n));
            message.success("Đã đọc thông báo");
        }
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('fullName');
        message.success('Đã đăng xuất!');
        navigate('/login');
    };

    const getMenuItems = () => {
        if (role === 'Admin') return [
            { key: '/admin', icon: <UserOutlined />, label: 'Tài khoản' },
            { key: '/admin/departments', icon: <TeamOutlined />, label: 'Phòng ban' },
            { key: '/admin/settings', icon: <SettingOutlined />, label: 'Cấu hình hệ thống' },
            { key: '/admin/audit-log', icon: <FileTextOutlined />, label: 'Audit Log' },
        ];
        if (role === 'Manager') return [
            { key: '/manager', icon: <DashboardOutlined />, label: 'Dashboard' },
            { key: '/manager/projects', icon: <ProjectOutlined />, label: 'Quản lý Dự án' },
            { key: '/manager/approvals', icon: <CheckSquareOutlined />, label: 'Phê duyệt' },
            { key: '/manager/kpi-review', icon: <SettingOutlined />, label: 'Chốt KPI Cuối Tháng' },
        ];
        if (role === 'C-Level') return [
            { key: '/c-level', icon: <DashboardOutlined />, label: 'C-Level Dashboard' },
        ];
        return [
            { key: '/employee', icon: <DashboardOutlined />, label: 'Trang chủ' },
            { key: '/employee/tasks', icon: <ProjectOutlined />, label: 'Công việc' },
            { key: '/employee/log-time', icon: <FileTextOutlined />, label: 'Lịch sử Log-time' },
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
                            title={<span style={{ fontWeight: item.isRead ? 'normal' : 'bold', fontSize: '13px' }}>{item.content}</span>} 
                            description={<span style={{ fontSize: '11px' }}>{item.createdAt ? new Date(item.createdAt).toLocaleDateString() : ''}</span>} 
                        />
                    </List.Item>
                )}
            />
        </Card>
    );

    const handleNotificationClick = (item) => {
        if (!item.isRead) {
            markAsRead(item.notifId);
        } else {
            message.info("Thông báo này đã được đọc");
        }
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
                            <Badge count={notifications.filter(n => !n.isRead).length} size="small">
                                <BellOutlined style={{ fontSize: '20px', cursor: 'pointer' }} />
                            </Badge>
                        </Dropdown>
                        <Dropdown menu={userMenu} placement="bottomRight">
                            <span style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8 }}>
                                <Avatar icon={<UserOutlined />} />
                                <span>{fullName}</span>
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
