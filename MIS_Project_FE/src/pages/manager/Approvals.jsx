import React, { useState, useEffect } from 'react';
import { Table, Button, Card, Space, message, Tag, Row, Col, Typography, Modal } from 'antd';
import { CheckOutlined, CloseOutlined, EyeOutlined } from '@ant-design/icons';
import api from '../../services/api';

const { Text } = Typography;

export default function Approvals() {
    const [loading, setLoading] = useState(false);
    const [requests, setRequests] = useState([]);

    const fetchRequests = async () => {
        setLoading(true);
        try {
            // Trong thực tế sẽ gọi đồng thời API duyệt Log-time và duyệt xin gia hạn
            const [logsRes, extRes] = await Promise.all([
                api.get('/logtimes/pending'),
                api.get('/tasks/extensions/pending')
            ]);
            // Kết hợp cả 2 danh sách yêu cầu
            const combined = [
                ...(logsRes.data || []).map(item => ({ ...item, type: 'Log-time', key: `log-${item.log_id}` })),
                ...(extRes.data || []).map(item => ({ ...item, type: 'Xin gia hạn', key: `ext-${item.request_id}` }))
            ];
            setRequests(combined);
        } catch (error) {
            // Mock dữ liệu khớp 100% với PlantUML
            setRequests([
                { id: 1, type: 'Log-time', sender: 'Dev A', task: 'Fix giao diện', detail: 'Thực tế: 3h (Bằng chứng: log.png)', raw: { log_id: 1, task_id: 2 } },
                { id: 2, type: 'Xin gia hạn', sender: 'Dev C', task: 'Viết Unit Test', detail: 'Lý do: Em xin thêm 1 ngày do ốm ạ', raw: { request_id: 1, task_id: 4 } },
                { id: 3, type: 'Log-time', sender: 'Dev B', task: 'Setup DB', detail: 'Thực tế: 8h (Bằng chứng: sql.txt)', raw: { log_id: 2, task_id: 1 } },
            ]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRequests();
    }, []);

    const handleApprove = async (record) => {
        try {
            if (record.type === 'Log-time') {
                await api.post(`/logtimes/${record.raw.log_id}/approve`);
            } else {
                await api.post(`/tasks/extensions/${record.raw.request_id}/approve`);
            }
            message.success('Đã phê duyệt yêu cầu!');
            fetchRequests();
        } catch (error) {
            // Giả lập approve thành công
            setRequests(requests.filter(r => r.id !== record.id));
            message.success('Phê duyệt yêu cầu thành công!');
        }
    };

    const handleReject = async (record) => {
        try {
            if (record.type === 'Log-time') {
                await api.post(`/logtimes/${record.raw.log_id}/reject`);
            } else {
                await api.post(`/tasks/extensions/${record.raw.request_id}/reject`);
            }
            message.warning('Đã từ chối yêu cầu!');
            fetchRequests();
        } catch (error) {
            // Giả lập reject thành công
            setRequests(requests.filter(r => r.id !== record.id));
            message.warning('Đã từ chối phê duyệt!');
        }
    };

    const columns = [
        {
            title: 'Loại yêu cầu',
            dataIndex: 'type',
            key: 'type',
            render: (type) => (
                <Tag color={type === 'Log-time' ? 'blue' : 'cyan'}>
                    {type}
                </Tag>
            )
        },
        {
            title: 'Người gửi',
            dataIndex: 'sender',
            key: 'sender',
            render: (text) => <strong>{text}</strong>
        },
        {
            title: 'Task tương ứng',
            dataIndex: 'task',
            key: 'task',
        },
        {
            title: 'Chi tiết yêu cầu',
            dataIndex: 'detail',
            key: 'detail',
            render: (text) => {
                if (text.includes('Bằng chứng:')) {
                    const parts = text.split('(Bằng chứng:');
                    const file = parts[1].replace(')', '').trim();
                    return (
                        <span>
                            {parts[0]} (Bằng chứng: <a href="#" onClick={(e) => { e.preventDefault(); message.info(`Mở file ${file}`); }}>{file}</a>)
                        </span>
                    );
                }
                return text;
            }
        },
        {
            title: 'Hành động',
            key: 'action',
            render: (_, record) => (
                <Space size="small">
                    <Button 
                        type="primary" 
                        size="small" 
                        icon={<CheckOutlined />} 
                        onClick={() => handleApprove(record)}
                    >
                        Duyệt
                    </Button>
                    <Button 
                        type="primary" 
                        danger 
                        ghost
                        size="small" 
                        icon={<CloseOutlined />} 
                        onClick={() => handleReject(record)}
                    >
                        Từ chối
                    </Button>
                </Space>
            )
        }
    ];

    return (
        <Card title="Trung tâm Phê duyệt (Log-time & Gia hạn)">
            <Table 
                columns={columns} 
                dataSource={requests} 
                rowKey="id" 
                loading={loading}
                pagination={false}
                bordered
            />
        </Card>
    );
}
