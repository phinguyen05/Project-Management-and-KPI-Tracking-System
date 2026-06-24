import React, { useState, useEffect } from 'react';
import { Table, Button, Card, Space, Tag, message, Popconfirm, Tabs } from 'antd';
import { CheckOutlined, CloseOutlined } from '@ant-design/icons';
import api from '../../services/api';


export default function ApprovalCenter() {
    const [pendingLogs, setPendingLogs] = useState([]);
    const [loadingLogs, setLoadingLogs] = useState(false);

    const [pendingLeaves, setPendingLeaves] = useState([]);
    const [loadingLeaves, setLoadingLeaves] = useState(false);


    const fetchPendingLogs = async () => {
        setLoadingLogs(true);

        try {
            const response = await api.get('/logtimes/pending');
            setPendingLogs(response.data);
        } catch (error) {
            message.error('Không thể tải danh sách chờ duyệt!');
        } finally {
            setLoadingLogs(false);
        }
    };


    const fetchPendingLeaves = async () => {
        setLoadingLeaves(true);
        try {
            const response = await api.get('/leavedays');
            const all = Array.isArray(response.data) ? response.data : [];
            setPendingLeaves(all.filter((x) => x.status === 'Pending'));
        } catch (error) {
            message.error('Không thể tải danh sách nghỉ phép!');
        } finally {
            setLoadingLeaves(false);
        }
    };

    const handleLeaveStatus = async (id, status) => {
        try {
            await api.put(`/leavedays/${id}/status`, { status });
            message.success(`Đã ${status === 'Approved' ? 'duyệt' : 'từ chối'} đơn nghỉ phép thành công!`);
            fetchPendingLeaves();
        } catch (error) {
            message.error('Lỗi khi xử lý!');
        }
    };

    useEffect(() => {
        fetchPendingLogs();
        fetchPendingLeaves();
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

    const logColumns = [
        { title: 'Người nộp', dataIndex: 'userName', key: 'userName', fontWeight: 'bold' },
        { title: 'Task tương ứng', dataIndex: 'taskName', key: 'taskName' },
        { title: 'Số giờ', dataIndex: 'actualHours', key: 'actualHours', render: (h) => <Tag color="blue">{h}h</Tag> },
        { title: 'Ngày làm', dataIndex: 'logDate', key: 'logDate' },
        {
            title: 'Tệp đính kèm',
            dataIndex: 'attachedFile',
            key: 'attachedFile',
            render: (file) => {
                const baseUrl = import.meta.env.VITE_API_BASE_URL.replace('/api', '');
                return file ? (
                    <a href={`${baseUrl}${file}`} target="_blank" rel="noreferrer">Tải file</a>
                ) : (
                    <span style={{ color: '#999' }}>—</span>
                );
            }
        },
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

    const leaveColumns = [
        { title: 'Người nộp', dataIndex: 'userName', key: 'userName', fontWeight: 'bold' },
        { title: 'Ngày nghỉ', dataIndex: 'leaveDate', key: 'leaveDate' },
        { title: 'Lý do', dataIndex: 'reason', key: 'reason' },
        { title: 'Trạng thái', dataIndex: 'status', key: 'status', render: (s) => <Tag color={s === 'Pending' ? 'processing' : s === 'Approved' ? 'success' : 'error'}>{s}</Tag> },
        {
            title: 'Hành động',
            key: 'action',
            render: (_, record) => (
                <Space size="middle">
                    <Popconfirm
                        title="Bạn có chắc muốn DUYỆT đơn nghỉ phép này?"
                        onConfirm={() => handleLeaveStatus(record.leaveId, 'Approved')}
                    >
                        <Button type="primary" size="small" icon={<CheckOutlined />} style={{ background: '#52c41a' }}>
                            Duyệt
                        </Button>
                    </Popconfirm>
                    <Popconfirm
                        title="Bạn muốn TỪ CHỐI đơn nghỉ phép này?"
                        onConfirm={() => handleLeaveStatus(record.leaveId, 'Rejected')}
                    >
                        <Button type="primary" danger size="small" icon={<CloseOutlined />}>
                            Từ chối
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <Card title="Trung tâm Phê duyệt Yêu cầu">
            <Tabs
                defaultActiveKey="logs"
                items={[
                    {
                        key: 'logs',
                        label: 'Duyệt Log-time',
                        children: (
                            <Table
                                columns={logColumns}
                                dataSource={pendingLogs}
                                rowKey="logId"
                                loading={loadingLogs}
                                pagination={false}
                                bordered
                            />
                        ),
                    },
                    {
                        key: 'leaves',
                        label: 'Duyệt Nghỉ Phép',
                        children: (
                            <Table
                                columns={leaveColumns}
                                dataSource={pendingLeaves}
                                rowKey="leaveId"
                                loading={loadingLeaves}
                                pagination={false}
                                bordered
                            />
                        ),
                    },
                ]}
            />
        </Card>
    );
}

