import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Card, Table, message, Row, Col, InputNumber, Select, Space } from 'antd';
import { SaveOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function SystemSettings() {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);
    const [holidays, setHolidays] = useState([]);
    const [holidayLoading, setHolidayLoading] = useState(false);
    const [holidayForm] = Form.useForm();

    const fetchConfig = async () => {
        setLoading(true);
        try {
            const response = await api.get('/systemconfig');
            if (response.data) {
                let penalty = {};
                try {
                    penalty = typeof response.data.penalty_factor === 'string' 
                        ? JSON.parse(response.data.penalty_factor) 
                        : (response.data.penalty_factor || {});
                } catch (e) {
                    penalty = { penalty_1_2_days: 0.8, penalty_3_5_days: 0.5, penalty_over_5_days: 0 };
                }

                form.setFieldsValue({
                    month_year: response.data.month_year || '06/2026',
                    standard_working_hours: response.data.standard_working_hours || 160,
                    penalty_1_2_days: penalty.penalty_1_2_days ?? 0.8,
                    penalty_3_5_days: penalty.penalty_3_5_days ?? 0.5,
                    penalty_over_5_days: penalty.penalty_over_5_days ?? 0.0,
                });
            }
        } catch (error) {
            message.warning('Chưa có cấu hình nào trong hệ thống hoặc API lỗi. Sử dụng cấu hình mặc định.');
            form.setFieldsValue({
                month_year: '06/2026',
                standard_working_hours: 176,
                penalty_1_2_days: 0.8,
                penalty_3_5_days: 0.5,
                penalty_over_5_days: 0.0
            });
        } finally {
            setLoading(false);
        }
    };

    const fetchHolidays = async () => {
        setHolidayLoading(true);
        try {
            const response = await api.get('/systemconfig/holidays');
            setHolidays(response.data || []);
        } catch (error) {
            setHolidays([
                { id: 1, holiday_date: '2026-06-01', description: 'Ngày Quốc tế Thiếu nhi' }
            ]);
        } finally {
            setHolidayLoading(false);
        }
    };

    useEffect(() => {
        fetchConfig();
        fetchHolidays();
    }, []);

    const handleSaveConfig = async (values) => {
        setLoading(true);
        try {
            const penalty_factor = JSON.stringify({
                penalty_1_2_days: values.penalty_1_2_days,
                penalty_3_5_days: values.penalty_3_5_days,
                penalty_over_5_days: values.penalty_over_5_days,
            });

            const payload = {
                month_year: values.month_year,
                standard_working_hours: values.standard_working_hours,
                penalty_factor: penalty_factor
            };

            await api.post('/systemconfig', payload);
            message.success('Cập nhật cấu hình hệ thống thành công!');
        } catch (error) {
            message.error('Lưu cấu hình hệ thống thất bại!');
        } finally {
            setLoading(false);
        }
    };

    const handleAddHoliday = async (values) => {
        try {
            await api.post('/systemconfig/holidays', values);
            message.success('Thêm ngày nghỉ lễ thành công!');
            holidayForm.resetFields();
            fetchHolidays();
        } catch (error) {
            message.error('Thao tác thất bại!');
        }
    };

    const handleDeleteHoliday = async (id) => {
        try {
            await api.delete(`/systemconfig/holidays/${id}`);
            message.success('Đã xóa ngày nghỉ lễ!');
            fetchHolidays();
        } catch (error) {
            message.error('Xóa ngày nghỉ lễ thất bại!');
        }
    };

    const holidayColumns = [
        { title: 'Ngày nghỉ lễ', dataIndex: 'holiday_date', key: 'holiday_date' },
        { title: 'Mô tả', dataIndex: 'description', key: 'description' },
        {
            title: 'Hành động',
            key: 'action',
            render: (_, record) => (
                <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDeleteHoliday(record.id)}>Xóa</Button>
            )
        }
    ];

    return (
        <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
                <Card title="Cấu hình tham số & Kỳ đánh giá" loading={loading}>
                    <Form form={form} layout="vertical" onFinish={handleSaveConfig}>
                        <Form.Item name="month_year" label="Tháng áp dụng" rules={[{ required: true }]}>
                            <Input placeholder="Ví dụ: 06/2026" />
                        </Form.Item>
                        <Form.Item name="standard_working_hours" label="Số ngày/giờ làm việc chuẩn trong tháng (Giờ)" rules={[{ required: true }]}>
                            <InputNumber style={{ width: '100%' }} min={1} />
                        </Form.Item>
                        
                        <h4 style={{ margin: '16px 0 8px 0', borderBottom: '1px solid #f0f0f0', paddingBottom: 8 }}>Hệ số phạt trễ hạn KPI</h4>
                        <Row gutter={8}>
                            <Col span={8}>
                                <Form.Item name="penalty_1_2_days" label="Trễ 1-2 ngày" rules={[{ required: true }]}>
                                    <InputNumber style={{ width: '100%' }} step={0.1} min={0} max={1} />
                                </Form.Item>
                            </Col>
                            <Col span={8}>
                                <Form.Item name="penalty_3_5_days" label="Trễ 3-5 ngày" rules={[{ required: true }]}>
                                    <InputNumber style={{ width: '100%' }} step={0.1} min={0} max={1} />
                                </Form.Item>
                            </Col>
                            <Col span={8}>
                                <Form.Item name="penalty_over_5_days" label="Trễ > 5 ngày" rules={[{ required: true }]}>
                                    <InputNumber style={{ width: '100%' }} step={0.1} min={0} max={1} />
                                </Form.Item>
                            </Col>
                        </Row>

                        <Form.Item>
                            <Button type="primary" htmlType="submit" icon={<SaveOutlined />} block>
                                Lưu cấu hình
                            </Button>
                        </Form.Item>
                    </Form>
                </Card>
            </Col>
            
            <Col xs={24} md={12}>
                <Card title="Quản lý ngày nghỉ lễ (Giảm trừ KPI Khối lượng)">
                    <Form form={holidayForm} layout="inline" onFinish={handleAddHoliday} style={{ marginBottom: 16 }}>
                        <Form.Item name="holiday_date" rules={[{ required: true, message: 'Nhập ngày!' }]}>
                            <Input type="date" style={{ width: 150 }} />
                        </Form.Item>
                        <Form.Item name="description" rules={[{ required: true, message: 'Nhập mô tả!' }]}>
                            <Input placeholder="Mô tả ngày lễ" style={{ width: 180 }} />
                        </Form.Item>
                        <Form.Item>
                            <Button type="primary" htmlType="submit" icon={<PlusOutlined />}>Thêm</Button>
                        </Form.Item>
                    </Form>

                    <Table 
                        dataSource={holidays} 
                        columns={holidayColumns} 
                        rowKey="id" 
                        loading={holidayLoading} 
                        size="small" 
                        pagination={{ pageSize: 5 }}
                    />
                </Card>
            </Col>
        </Row>
    );
}
