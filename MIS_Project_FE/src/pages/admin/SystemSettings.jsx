import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Card, message, Row, Col, InputNumber } from 'antd';
import { SaveOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function SystemSettings() {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);


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



    useEffect(() => {
        fetchConfig();
    }, []);

    const handleSaveConfig = async (values) => {
        setLoading(true);
        try {
            const payload = {
                MonthYear: String(values.month_year),
                StandardWorkingHours: parseInt(values.standard_working_hours, 10),
                Holidays: 0,
                PenaltyFactor: JSON.stringify({
                    penalty_1_2_days: parseFloat(values.penalty_1_2_days),
                    penalty_3_5_days: parseFloat(values.penalty_3_5_days),
                    penalty_over_5_days: parseFloat(values.penalty_over_5_days),
                }),
            };

            await api.post('/SystemConfig', payload);
            message.success('Cập nhật cấu hình hệ thống thành công!');
        } catch (error) {
            message.error('Lưu cấu hình hệ thống thất bại!');
        } finally {
            setLoading(false);
        }
    };



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
            
        </Row>
    );
}
