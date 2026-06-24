import React, { useEffect, useState } from 'react';
import { Table, Typography, message } from 'antd';
import api from '../../services/api';

const { Title } = Typography;

const CLevelDashboard = () => {
    const [loading, setLoading] = useState(false);
    const [projects, setProjects] = useState([]);

    useEffect(() => {
        const fetchProjects = async () => {
            setLoading(true);
            try {
                const res = await api.get('/projects');
                const data = res?.data?.items ?? res?.data ?? [];
                setProjects(Array.isArray(data) ? data : []);
            } catch (err) {
                console.error('[CLevelDashboard] fetchProjects error:', err);
                message.error('Không thể tải danh sách dự án');
                setProjects([]);
            } finally {
                setLoading(false);
            }
        };

        fetchProjects();
    }, []);

    const formatDate = (value) => {
        if (!value) return '-';
        const d = new Date(value);
        if (Number.isNaN(d.getTime())) return '-';
        return d.toLocaleDateString();
    };

    const columns = [
        {
            title: 'Tên dự án',
            dataIndex: 'name',
            key: 'name',
            render: (_, record) =>
                record?.name ?? record?.projectName ?? record?.title ?? '-',
        },
        {
            title: 'Mô tả',
            dataIndex: 'description',
            key: 'description',
            render: (_, record) =>
                record?.description ?? record?.summary ?? '-',
        },
        {
            title: 'Ngày bắt đầu',
            dataIndex: 'startDate',
            key: 'startDate',
            render: (_, record) =>
                formatDate(record?.startDate ?? record?.beginDate ?? record?.start_date),
        },
        {
            title: 'Ngày kết thúc',
            dataIndex: 'endDate',
            key: 'endDate',
            render: (_, record) =>
                formatDate(record?.endDate ?? record?.end_date ?? record?.finishDate),
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            render: (_, record) => record?.status ?? record?.state ?? '-',
        },
    ];

    return (
        <div style={{ padding: '10px' }}>
            <Title level={2} style={{ marginBottom: 16 }}>
                Tổng quan Dự án (View-only)
            </Title>

            <Table
                rowKey={(record) => record?.id ?? record?.projectId ?? JSON.stringify(record)}
                loading={loading}
                columns={columns}
                dataSource={projects}
                pagination={{ pageSize: 10, showSizeChanger: true }}
            />
        </div>
    );
};

export default CLevelDashboard;

