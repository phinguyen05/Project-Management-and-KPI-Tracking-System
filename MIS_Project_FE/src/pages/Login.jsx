import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Checkbox, message, Card } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import api from '../services/api';

export default function Login() {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        // Nếu đã có token hợp lệ, chuyển hướng thẳng vào dashboard tương ứng
        const token = localStorage.getItem('token');
        const role = localStorage.getItem('role');
        if (token && role) {
            try {
                const decoded = jwtDecode(token);
                // Kiểm tra token đã hết hạn chưa
                if (decoded.exp * 1000 > Date.now()) {
                    redirectByRole(role);
                } else {
                    // Token hết hạn thì xóa đi
                    localStorage.removeItem('token');
                    localStorage.removeItem('role');
                }
            } catch (e) {
                localStorage.removeItem('token');
                localStorage.removeItem('role');
            }
        }
    }, []);

    const redirectByRole = (role) => {
        if (role === 'Admin') navigate('/admin');
        else if (role === 'Manager') navigate('/manager');
        else if (role === 'C-Level') navigate('/c-level');
        else navigate('/employee');
    };

    const onFinish = async (values) => {
        setLoading(true);
        try {
            const response = await api.post('/auth/login', {
                username: values.username,
                password: values.password
            });
            
            const token = response.data.token;
            localStorage.setItem('token', token);
            
            const decoded = jwtDecode(token);
            const role = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            localStorage.setItem('role', role);

            message.success('Đăng nhập thành công!');
            redirectByRole(role);

        } catch (error) {
            message.error(error.response?.data || 'Sai tên đăng nhập hoặc mật khẩu!');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', background: '#f0f2f5' }}>
            <Card title="HỆ THỐNG QUẢN LÝ DỰ ÁN & KPI" style={{ width: 400, textAlign: 'center', borderRadius: 8, boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}>
                <h3 style={{ marginBottom: 24, color: '#1890ff' }}>ĐĂNG NHẬP</h3>
                <Form name="login" onFinish={onFinish} layout="vertical">
                    <Form.Item name="username" rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập!' }]}>
                        <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" size="large" />
                    </Form.Item>

                    <Form.Item name="password" rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}>
                        <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" size="large" />
                    </Form.Item>

                    <Form.Item name="remember" valuePropName="checked" style={{ textAlign: 'left' }}>
                        <Checkbox>Ghi nhớ đăng nhập</Checkbox>
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={loading} block size="large" style={{ marginBottom: 16 }}>Đăng nhập</Button>
                        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <a href="#" onClick={(e) => { e.preventDefault(); message.info('Vui lòng liên hệ Admin để đặt lại mật khẩu!'); }}>Quên mật khẩu?</a>
                            <a href="#" onClick={(e) => { e.preventDefault(); message.info('Vui lòng đăng nhập và đổi mật khẩu trong phần cá nhân!'); }}>Đổi mật khẩu</a>
                        </div>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
}
