import React, { useState, useEffect } from 'react';
import { Table, DatePicker, Input, Button, Space, Tag, message } from 'antd';
import api from '../../services/api';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

export default function AuditLogPage() {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(false);
    const [filterUser, setFilterUser] = useState('');
    const [filterDateRange, setFilterDateRange] = useState([]);

    useEffect(() => {
        fetchAuditLogs();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [filterUser, filterDateRange]);

    const fetchAuditLogs = async () => {
        setLoading(true);
        try {
            const response = await api.get('/auditlogs');
            let filteredLogs = response.data;

            if (filterUser) {
                filteredLogs = filteredLogs.filter(log =>
                    log.userName.toLowerCase().includes(filterUser.toLowerCase())
                );
            }

            if (filterDateRange.length === 2) {
                const [startDate, endDate] = filterDateRange;
                filteredLogs = filteredLogs.filter(log => {
                    const logDate = dayjs(log.createdAt);
                    return logDate.isAfter(startDate.subtract(1, 'day')) && logDate.isBefore(endDate.add(1, 'day'));
                });
            }

            setLogs(filteredLogs);
        } catch (error) {
            message.error('Lỗi khi tải nhật ký thao tác!');
            setLogs([]);
        } finally {
            setLoading(false);
        }
    };

    const columns = [
        { title: 'Thời gian', dataIndex: 'createdAt', key: 'createdAt', render: (text) => dayjs(text).format('YYYY-MM-DD HH:mm') },
        { title: 'Người thực hiện', dataIndex: 'userName', key: 'userName' },
        {
            title: 'Hành động',
            dataIndex: 'action',
            key: 'action',
            render: (action) => {
                let color = 'geekblue';
                if (action === 'DELETE') color = 'volcano';
                if (action === 'UPDATE') color = 'gold';
                return <Tag color={color}>{action.toUpperCase()}</Tag>;
            }
        },
        { title: 'Chi tiết thay đổi', dataIndex: 'details', key: 'details' },
    ];

    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Nhật ký Thao Tác (Audit Log)</h2>
            <div style={{ marginBottom: 16 }}>
                <Space>
                    <Input
                        placeholder="Nhập tên user..."
                        value={filterUser}
                        onChange={(e) => setFilterUser(e.target.value)}
                        style={{ width: 200 }}
                    />
                    <RangePicker
                        value={filterDateRange}
                        onChange={(dates) => setFilterDateRange(dates)}
                        format="DD/MM/YYYY"
                    />
                    <Button type="primary" onClick={fetchAuditLogs}>Lọc</Button>
                </Space>
            </div>
            <Table dataSource={logs} columns={columns} rowKey="logId" loading={loading} />
        </div>
    );
}
