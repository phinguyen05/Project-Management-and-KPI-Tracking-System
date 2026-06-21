import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Select, message, Popconfirm, Space } from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined, UserOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function DepartmentManagement() {
    const [departments, setDepartments] = useState([]);
    const [users, setUsers] = useState([]);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [editingDept, setEditingDept] = useState(null);
    const [form] = Form.useForm();

    useEffect(() => {
        fetchDepartments();
        fetchUsers();
    }, []);

    const fetchDepartments = async () => {
        try {
            const response = await api.get('/department'); // Sửa thành /department (số ít)
            setDepartments(response.data);
        } catch (error) {
            message.error('Lỗi khi tải danh sách phòng ban!');
        }
    };

    const fetchUsers = async () => {
        try {
            const response = await api.get('/user');
            setUsers(response.data.filter(u => u.role === 'Manager' || u.role === 'Employee'));
        } catch (error) {
            message.error('Lỗi khi tải danh sách người dùng!');
        }
    };

    const handleAdd = () => {
        setEditingDept(null);
        form.resetFields();
        setIsModalVisible(true);
    };

    const handleEdit = (record) => {
        setEditingDept(record);
        form.setFieldsValue({ ...record, managerId: record.managerId || undefined });
        setIsModalVisible(true);
    };

    const handleDelete = async (deptId) => {
        try {
            await api.delete(`/department/${deptId}`); // Sửa thành /department (số ít)
            message.success('Xóa phòng ban thành công!');
            fetchDepartments();
        } catch (error) {
            message.error(error.response?.data || 'Lỗi khi xóa phòng ban!');
        }
    };

    const handleSave = async () => {
        try {
            const values = await form.validateFields();
            if (editingDept) {
                await api.put(`/department/${editingDept.departmentId}`, values);
                message.success('Cập nhật phòng ban thành công!');
            } else {
                await api.post('/department', values);
                message.success('Thêm phòng ban thành công!');
            }
            setIsModalVisible(false);
            fetchDepartments();
        } catch (error) {
            message.error(error.response?.data || 'Lỗi khi lưu phòng ban!');
        }
    };

    const columns = [
        { title: 'Tên Phòng Ban', dataIndex: 'name', key: 'name' },
        {
            title: 'Trưởng Phòng',
            dataIndex: 'managerId',
            key: 'managerId',
            render: (managerId) => {
                const manager = users.find(u => u.userId === managerId);
                return manager ? manager.fullName : 'Chưa chỉ định';
            },
        },
        {
            title: 'Số Nhân Sự',
            dataIndex: 'memberCount', // Cần Backend trả về trường này
            key: 'memberCount',
            render: (text, record) => {
                const count = users.filter(u => u.departmentId === record.departmentId).length;
                return count; // Đếm số lượng user trong phòng ban này
            }
        },
        {
            title: 'Hành Động',
            key: 'actions',
            render: (_, record) => (
                <Space size="middle">
                    <Button icon={<EditOutlined />} onClick={() => handleEdit(record)} />
                    <Popconfirm title="Bạn có chắc muốn xóa?" onConfirm={() => handleDelete(record.departmentId)} okText="Có" cancelText="Không">
                        <Button icon={<DeleteOutlined />} danger />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Quản lý Phòng Ban</h2>
            <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd} style={{ marginBottom: 16 }}>
                Thêm Phòng Ban Mới
            </Button>
            <Table dataSource={departments} columns={columns} rowKey="departmentId" />

            <Modal
                title={editingDept ? 'Sửa Phòng Ban' : 'Thêm Phòng Ban'}
                open={isModalVisible}
                onOk={handleSave}
                onCancel={() => setIsModalVisible(false)}
                destroyOnClose
            >
                <Form form={form} layout="vertical" name="department_form">
                    <Form.Item name="name" label="Tên Phòng Ban" rules={[{ required: true, message: 'Vui lòng nhập tên phòng ban!' }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="managerId" label="Trưởng Phòng" >
                        <Select placeholder="Chọn trưởng phòng (nếu có)" allowClear>
                            {users.map(user => (
                                <Select.Option key={user.userId} value={user.userId}>
                                    <Space><Avatar size="small" icon={<UserOutlined />} />{user.fullName} ({user.role})</Space>
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
}
