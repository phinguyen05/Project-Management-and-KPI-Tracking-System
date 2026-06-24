import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, InputNumber, DatePicker, message, Tag, Space, Popconfirm, Input } from 'antd';
import { EditOutlined, DeleteOutlined, CheckOutlined, CloseOutlined } from '@ant-design/icons';
import api from '../../services/api';
import dayjs from 'dayjs';

export default function LogTimeHistory() {
    const [logTimes, setLogTimes] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [editingLog, setEditingLog] = useState(null);
    const [form] = Form.useForm();

    useEffect(() => {
        fetchLogTimes();
    }, []);

    const fetchLogTimes = async () => {
        setLoading(true);
        try {
            const response = await api.get('/logtimes/me');
            setLogTimes(Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            message.error(error?.response?.data || "Lỗi khi tải lịch sử Log-time!");
            setLogTimes([]);
        } finally {
            setLoading(false);
        }
    };

    const handleEdit = (record) => {
        setEditingLog(record);
        form.setFieldsValue({
            ...record,
            logDate: dayjs(record.logDate),
        });
        setIsModalVisible(true);
    };

    const handleCancelLog = async (logId) => {
        try {
            // Cần tạo API /logtimes/{id}/cancel ở Backend
            await api.patch(`/logtimes/${logId}/cancel`);
            message.success("Hủy báo cáo giờ làm thành công!");
            fetchLogTimes();
        } catch (error) {
            message.error(error.response?.data || "Lỗi khi hủy báo cáo giờ làm!");
        }
    };

    const handleSave = async () => {
        try {
            const values = await form.validateFields();
            const payload = {
                actualHours: values.actualHours,
                logDate: values.logDate.format("YYYY-MM-DD"),
                description: values.description,
                // attachedFile: "" // Cần bổ sung upload file
            };
            await api.put(`/logtimes/${editingLog.logId}`, payload); // Cần tạo API PUT /logtimes/{id} ở Backend
            message.success("Cập nhật báo cáo giờ làm thành công!");
            setIsModalVisible(false);
            fetchLogTimes();
        } catch (error) {
            message.error(error.response?.data || "Lỗi khi cập nhật báo cáo giờ làm!");
        }
    };

    const getStatusTag = (status) => {
        switch (status) {
            case "Approved": return <Tag icon={<CheckOutlined />} color="success">Đã duyệt</Tag>;
            case "Pending": return <Tag icon={<CloseOutlined />} color="processing">Chờ duyệt</Tag>;
            case "Rejected": return <Tag icon={<CloseOutlined />} color="error">Từ chối</Tag>;
            default: return <Tag>{status}</Tag>;
        }
    };

    const columns = [
        { title: "Task", dataIndex: "taskName", key: "taskName" },
        { title: "Số giờ", dataIndex: "actualHours", key: "actualHours" },
        { title: "Ngày làm", dataIndex: "logDate", key: "logDate", render: (text) => dayjs(text).format("DD/MM/YYYY") },
        { title: "Trạng thái", dataIndex: "status", key: "status", render: getStatusTag },
        { title: "Mô tả", dataIndex: "description", key: "description" },
        {
            title: "Hành động",
            key: "actions",
            render: (_, record) => (
                <Space size="middle">
                    <Button 
                        icon={<EditOutlined />} 
                        onClick={() => handleEdit(record)} 
                        disabled={record.status !== "Pending"}
                    />
                    <Popconfirm
                        title="Bạn có chắc muốn hủy báo cáo này?"
                        onConfirm={() => handleCancelLog(record.logId)}
                        okText="Có"
                        cancelText="Không"
                        disabled={record.status !== "Pending"}
                    >
                        <Button 
                            icon={<DeleteOutlined />} 
                            danger 
                            disabled={record.status !== "Pending"}
                        />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Lịch Sử Báo Cáo Giờ Làm</h2>
            <Table dataSource={logTimes} columns={columns} rowKey="logId" loading={loading} />

            <Modal
                title="Cập nhật báo cáo giờ làm"
                open={isModalVisible}
                onOk={handleSave}
                onCancel={() => setIsModalVisible(false)}
                destroyOnClose
            >
                <Form form={form} layout="vertical" name="log_time_edit_form">
                    <Form.Item name="taskName" label="Task" >
                        <Input disabled />
                    </Form.Item>
                    <Form.Item name="actualHours" label="Số giờ làm việc thực tế (Giờ)" rules={[{ required: true, message: "Vui lòng nhập số giờ!" }]}>
                        <InputNumber min={0.5} max={24} style={{ width: "100%" }} placeholder="VD: 4.5" />
                    </Form.Item>
                    <Form.Item name="logDate" label="Ngày thực hiện" rules={[{ required: true, message: "Vui lòng chọn ngày!" }]}>
                        <DatePicker style={{ width: "100%" }} format="DD/MM/YYYY" />
                    </Form.Item>
                    <Form.Item name="description" label="Ghi chú (Tùy chọn)">
                        <Input.TextArea rows={2} placeholder="Nhập tóm tắt công việc đã làm..." />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
}
