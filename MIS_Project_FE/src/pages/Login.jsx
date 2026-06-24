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
                    localStorage.removeItem('fullName');
                    localStorage.removeItem('username');
                }
            } catch (e) {
                localStorage.removeItem('token');
                localStorage.removeItem('role');
                localStorage.removeItem('fullName');
                localStorage.removeItem('username');
            }
        }
    }, []);

    const normalizeRoleValue = (r) => {
        const s = (r ?? '').toString().trim();
        const upper = s.replace(/_/g, '-').replace(/\s+/g, '-').toUpperCase();

        // Canonical mapping
        if (upper === 'ADMIN') return 'Admin';
        if (upper === 'MANAGER') return 'Manager';
        if (upper === 'EMPLOYEE') return 'Employee';

        // C-Level variants -> canonical 'C-Level'
        if (upper === 'CLEVEL' || upper === 'C-LEVEL' || upper === 'C_LEVEL') return 'C-Level';

        return s ? s : '';
    };

    const redirectByRole = (rawRole) => {
        const role = normalizeRoleValue(rawRole);
        if (role === 'Admin') navigate('/admin');
        else if (role === 'Manager') navigate('/manager');
        else if (role === 'C-Level') navigate('/c-level');
        else if (role === 'Employee') navigate('/employee');
        else navigate('/unauthorized');
    };

    const extractUsernameFromJwtToken = (token) => {
        try {
            // Decode payload bằng javascript thuần (không phụ thuộc thư viện)
            const payloadBase64 = token.split('.')[1];
            // jwt uses base64url, cần thay thế '-'->'+' '_'->'/' và thêm '=' nếu thiếu
            const base64 = payloadBase64.replace(/-/g, '+').replace(/_/g, '/');
            const padded = base64.padEnd(base64.length + (4 - (base64.length % 4)) % 4, '=');

            const payload = JSON.parse(atob(padded));

            const extractedName =
                payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                payload.unique_name ||
                payload.name ||
                payload.sub ||
                'User';

            return extractedName ? extractedName.toString().trim() : '';
        } catch (e) {
            return '';
        }
    };

    const onFinish = async (values) => {
        setLoading(true);
        try {
            const response = await api.post('/auth/login', {
                username: values.username,
                password: values.password
            });

            const responseData = response?.data || {};

            const token = responseData?.token;
            if (!token) {
                throw new Error('API không trả về token.');
            }

            localStorage.setItem('token', token);

            const decoded = jwtDecode(token);

            const role =
                decoded?.role ||
                decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                decoded?.Role;

            if (!role) {
                throw new Error('Không đọc được role từ token.');
            }
            localStorage.setItem('role', role);

            // Lấy username/fullName từ JWT claims chuẩn .NET
            const extractedUsername = extractUsernameFromJwtToken(token);
            if (extractedUsername) {
                // Key PHẢI khớp với MainLayout.jsx
                localStorage.setItem('username', extractedUsername);
                // optional: giữ fullName để fallback nếu cần
                localStorage.setItem('fullName', extractedUsername);
            }

            message.success('Đăng nhập thành công!');
            redirectByRole(role);
        } catch (error) {
            const status = error?.response?.status;
            const data = error?.response?.data;
            console.error('[Login error]', { status, data, error });

            if (typeof data === 'string') {
                message.error(data);
            } else if (data?.message) {
                message.error(data.message);
            } else if (status === 401 || status === 400) {
                message.error('Sai tài khoản hoặc mật khẩu!');
            } else if (status === 403) {
                message.error(data?.message || 'Tài khoản bị khóa hoặc không đủ quyền.');
            } else {
                message.error('Không thể đăng nhập. Vui lòng kiểm tra API/Network.');
            }
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

