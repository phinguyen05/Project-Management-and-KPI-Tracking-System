import React, { useState, useEffect } from 'react';
import { Table, Button, Tag, Space, Card, Modal, Form, Input, Select, message, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, LockOutlined, UnlockOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function UserManagement() {
    const [users, setUsers] = useState([]);
    const [departments, setDepartments] = useState([]); // ĐÃ THÊM: State lưu danh sách phòng ban
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [form] = Form.useForm();

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const response = await api.get('/user');
            setUsers(response.data);
        } catch (error) {
            message.error('Không thể tải danh sách người dùng!');
        } finally {
            setLoading(false);
        }
    };

    // ĐÃ THÊM: Hàm kéo danh sách phòng ban từ Backend
   const fetchDepartments = async () => {
    try {
        console.log("Đang gọi API tới: /api/department"); // Log để xem trong F12
        const response = await api.get('/Department'); 
        console.log("Dữ liệu nhận về:", response.data); // Log dữ liệu nhận được
        setDepartments(response.data);
    } catch (error) {
        console.error('Lỗi chi tiết:', error);
        // Nếu lỗi là 401, nghĩa là Token chưa được gửi kèm
        if (error.response && error.response.status === 401) {
            message.error('Bạn chưa đăng nhập hoặc Token hết hạn!');
        } else {
            message.error('Không tìm thấy API phòng ban!');
        }
    }
};

    useEffect(() => {
        fetchUsers();
        fetchDepartments(); // Gọi thêm load phòng ban khi vào trang
    }, []);

    // SỬA LỖI SỐ 5: Chuẩn hóa Payload khi tạo User khớp với Backend
    const handleAddUser = async (values) => {
        try {
            const payload = {
                fullName: values.fullName,
                username: values.username,
                email: values.email,
                password: values.password,
                role: values.role,
                departmentId: values.departmentId ? parseInt(values.departmentId) : null
            };
            await api.post('/user', payload);
            message.success('Thêm nhân sự mới thành công!');
            setIsModalVisible(false);
            form.resetFields();
            fetchUsers();
        } catch (error) {
            message.error(error.response?.data?.message || error.response?.data || 'Lỗi khi tạo tài khoản!');
        }
    };

    // SỬA LỖI SỐ 4: Gắn đúng chữ userId khi gọi API Xóa
    const handleDelete = async (userId) => {
        if (!userId) {
            message.error('Lỗi: Không tìm thấy ID tài khoản!');
            return;
        }
        try {
            await api.delete(`/user/${userId}`);
            message.success('Đã xóa (khóa) tài khoản thành công!');
            fetchUsers();
        } catch (error) {
            message.error('Lỗi khi xóa tài khoản!');
        }
    };

    const handleToggleStatus = async (record) => {
        const newStatus = record.status === 'Active' ? 'Locked' : 'Active';
        try {
            await api.put(`/user/${record.userId}`, { ...record, status: newStatus });
            message.success(`Đã ${newStatus === 'Active' ? 'mở' : 'khóa'} tài khoản thành công!`);
            fetchUsers();
        } catch (error) {
            message.error('Lỗi khi cập nhật trạng thái!');
        }
    };

    // Chuẩn hóa tên cột (dataIndex) khớp với Backend trả về
    const columns = [
        { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName', fontWeight: 'bold' },
        { title: 'Tên đăng nhập', dataIndex: 'username', key: 'username' },
        { title: 'Email', dataIndex: 'email', key: 'email' },
        { 
            title: 'Phòng ban', 
            dataIndex: 'departmentId', 
            key: 'departmentId',
            // ĐÃ FIX: Map ID sang tên phòng ban để hiển thị ra bảng cho xịn
            render: (deptId) => {
                if (!deptId) return <span style={{ color: '#ccc' }}>Chưa gán</span>;
                // Phòng hờ Backend trả về id hoặc departmentId, name hoặc departmentName
                const dept = departments.find(d => d.id === deptId || d.departmentId === deptId);
                return dept ? (dept.name || dept.departmentName) : `ID Phòng: ${deptId}`;
            }
        },
        {
            title: 'Phân quyền', dataIndex: 'role', key: 'role',
            render: (role) => <Tag color={role === 'Manager' ? 'geekblue' : role === 'Admin' ? 'purple' : 'green'}>{role}</Tag>
        },
        {
            title: 'Trạng thái', dataIndex: 'status', key: 'status',
            render: (status) => <Tag color={status === 'Active' ? 'success' : 'error'}>{status === 'Active' ? 'Hoạt động' : 'Đã khóa'}</Tag>
        },
        {
            title: 'Hành động', key: 'action',
            render: (_, record) => (
                <Space size="middle">
                    <Button type="primary" size="small" icon={<EditOutlined />}>Sửa</Button>
                    {record.status === 'Active' ? (
                        <Popconfirm title="Khóa tài khoản này?" onConfirm={() => handleToggleStatus(record)}>
                            <Button type="default" danger size="small" icon={<LockOutlined />}>Khóa</Button>
                        </Popconfirm>
                    ) : (
                        <Popconfirm title="Mở lại tài khoản này?" onConfirm={() => handleToggleStatus(record)}>
                            <Button type="default" size="small" style={{color: 'green', borderColor: 'green'}} icon={<UnlockOutlined />}>Mở</Button>
                        </Popconfirm>
                    )}
                    <Popconfirm title="Xóa tài khoản này?" onConfirm={() => handleDelete(record.userId)}>
                        <Button type="primary" danger size="small" icon={<DeleteOutlined />}>Xóa</Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <Card title="Quản lý Tài khoản & Phân quyền" extra={<Button type="primary" icon={<PlusOutlined />} onClick={() => setIsModalVisible(true)}>Thêm nhân sự mới</Button>}>
            <Table columns={columns} dataSource={users} rowKey="userId" loading={loading} pagination={{ pageSize: 10 }} bordered />
            <Modal title="Thêm nhân sự mới" open={isModalVisible} onCancel={() => setIsModalVisible(false)} footer={null}>
                <Form layout="vertical" form={form} onFinish={handleAddUser}>
                    <Form.Item name="fullName" label="Họ tên" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="username" label="Tên đăng nhập" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="email" label="Email" rules={[{ type: 'email' }]}><Input /></Form.Item>
                    <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}><Input.Password /></Form.Item>
                    <Form.Item name="role" label="Phân quyền" rules={[{ required: true }]}>
                        <Select>
                            <Select.Option value="Employee">Nhân viên (Employee)</Select.Option>
                            <Select.Option value="Manager">Quản lý (Manager)</Select.Option>
                            <Select.Option value="Admin">Quản trị (Admin)</Select.Option>
                        </Select>
                    </Form.Item>
                    
                    {/* ĐÃ FIX: Đổi từ Input number nhập tay sang Select xổ xuống */}
                    <Form.Item name="departmentId" label="Phòng ban (Tùy chọn)">
                        <Select placeholder="Chọn phòng ban..." allowClear>
                            {departments.map(dept => (
                                <Select.Option key={dept.id || dept.departmentId} value={dept.id || dept.departmentId}>
                                    {dept.name || dept.departmentName}
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Form.Item><Button type="primary" htmlType="submit" block>Tạo tài khoản</Button></Form.Item>
                </Form>
            </Modal>
        </Card>
    );
}