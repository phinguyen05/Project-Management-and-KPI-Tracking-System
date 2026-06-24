import React, { useState } from 'react';
import { Card, Form, DatePicker, Input, Button, message } from 'antd';
import api from '../../services/api';
import dayjs from 'dayjs';


export default function LeaveRequest() {
    const [form] = Form.useForm();
    const [submitting, setSubmitting] = useState(false);

    const onFinish = async (values) => {
        // values.leaveRange là [start, end]
        const leaveRange = values.leaveRange;
        if (!leaveRange || !leaveRange[0] || !leaveRange[1]) {
            message.error('Vui lòng chọn khoảng ngày nghỉ.');
            return;
        }

        const start = leaveRange[0].startOf('day');
        const end = leaveRange[1].endOf('day');

        // Backend LeaveDaysController hiện tại chỉ nhận 1 ngày (LeaveDate)
        // => gửi từng ngày để tạo đầy đủ records Pending
        const dates = [];
        let cursor = start;
        while (cursor.isBefore(end) || cursor.isSame(end, 'day')) {
            dates.push(cursor);
            cursor = cursor.add(1, 'day');
        }

        setSubmitting(true);
        try {
            await Promise.all(
                dates.map((d) =>
                    api.post('/leavedays', {
                        leaveDate: d.toDate(),
                        reason: values.reason || '',
                    })
                )
            );

            message.success('Đã gửi đơn xin nghỉ phép.');
            form.resetFields();
        } catch (e) {
            message.error(e?.response?.data || 'Gửi đơn xin nghỉ phép thất bại.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Xin nghỉ phép</h2>

            <Card>
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={onFinish}
                    initialValues={{ reason: '' }}
                >
                    <Form.Item
                        name="leaveRange"
                        label="Chọn ngày nghỉ"
                        rules={[{ required: true, message: 'Vui lòng chọn khoảng ngày nghỉ!' }]}
                    >
                        <DatePicker.RangePicker
                            style={{ width: '100%' }}
                            disabledDate={(current) => current && current < dayjs().startOf('day')}
                        />
                    </Form.Item>


                    <Form.Item
                        name="reason"
                        label="Lý do"
                        rules={[{ required: true, message: 'Vui lòng nhập lý do!' }]}
                    >
                        <Input.TextArea rows={4} placeholder="Nhập lý do xin nghỉ..." />
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={submitting} block>
                            Gửi đơn nghỉ phép
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
}

