import React, { useState, useEffect } from 'react';
import { Table, Button, Card, Space, Tag, message, Popconfirm } from 'antd';
import { CheckOutlined, CloseOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function ApprovalCenter() {
    const [pendingLogs, setPendingLogs] = useState([]);
    const [loading, setLoading] = useState(false);

    const fetchPendingLogs = async () => {
        setLoading(true);
        try {
            const response = await api.get('/logtimes/pending');
            setPendingLogs(response.data);
        } catch (error) {
            message.error('Không thể tải danh sách chờ duyệt!');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPendingLogs();
    }, []);

    const handleApprove = async (id, status) => {
        try {
            await api.patch(`/logtimes/${id}/approve`, { status });
            message.success(`Đã ${status === 'Approved' ? 'duyệt' : 'từ chối'} báo cáo thành công!`);
            fetchPendingLogs(); // Tải lại bảng để làm biến mất dòng đã duyệt
        } catch (error) {
            message.error('Lỗi khi xử lý!');
        }
    };

    const columns = [
        { title: 'Người nộp', dataIndex: 'userName', key: 'userName', fontWeight: 'bold' },
        { title: 'Task tương ứng', dataIndex: 'taskName', key: 'taskName' },
        { title: 'Số giờ', dataIndex: 'actualHours', key: 'actualHours', render: (h) => <Tag color="blue">{h}h</Tag> },
        { title: 'Ngày làm', dataIndex: 'logDate', key: 'logDate' },
        { title: 'Ghi chú', dataIndex: 'description', key: 'description' },
        {
            title: 'Hành động', key: 'action',
            render: (_, record) => (
                <Space size="middle">
                    <Popconfirm title="Bạn có chắc muốn DUYỆT giờ làm này?" onConfirm={() => handleApprove(record.logId, 'Approved')}>
                        <Button type="primary" size="small" icon={<CheckOutlined />} style={{ background: '#52c41a' }}>Duyệt</Button>
                    </Popconfirm>
                    <Popconfirm title="Bạn muốn TỪ CHỐI giờ làm này?" onConfirm={() => handleApprove(record.logId, 'Rejected')}>
                        <Button type="primary" danger size="small" icon={<CloseOutlined />}>Từ chối</Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <Card title="Trung tâm Phê duyệt Yêu cầu">
            <Table columns={columns} dataSource={pendingLogs} rowKey="logId" loading={loading} pagination={false} bordered />
        </Card>
    );
}
