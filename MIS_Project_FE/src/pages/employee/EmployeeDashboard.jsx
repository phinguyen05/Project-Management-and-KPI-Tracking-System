import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Progress, List, Tag, Space, message } from 'antd'; // Đã import đầy đủ Space và Progress
import { ClockCircleOutlined } from '@ant-design/icons';
import api from '../../services/api';

export default function EmployeeDashboard() {
    const [kpi, setKpi] = useState({ timeliness: 0, efficiency: 0, capacity: 0, managerScore: 0 });
    const [dueTasks, setDueTasks] = useState([]);

    useEffect(() => {
        const init = async () => {
            // Tự động lấy userId từ Token claim ở backend (không phụ thuộc localStorage)


            try {
                const now = new Date();
                const month = now.getMonth() + 1;
                const year = now.getFullYear();

                // Load KPI
                const res = await api.get(`/kpis/me?month=${month}&year=${year}`);

                const data = res?.data || {};

                setKpi({
                    timeliness: data.kpi_timeliness ?? 0,
                    efficiency: data.kpi_efficiency ?? 0,
                    capacity: data.kpi_capacity ?? 0,
                    managerScore: data.kpi_manager_evaluation ?? 0,
                });

                // Load tasks and map due soon
                // Chỉ thị các task chưa Done và sắp theo deadline gần nhất
                const tasksRes = await api.get('/tasks/me');
                const tasks = Array.isArray(tasksRes?.data) ? tasksRes.data : [];


                const filtered = tasks
                    .filter(t => (t.status ?? '') !== 'Done')
                    .sort((a, b) => {
                        const da = a.deadline ? new Date(a.deadline).getTime() : Number.MAX_SAFE_INTEGER;
                        const db = b.deadline ? new Date(b.deadline).getTime() : Number.MAX_SAFE_INTEGER;
                        return da - db;
                    })
                    .map((t, idx) => ({
                        id: t.taskId ?? t.id ?? idx,
                        name: t.name ?? t.taskName ?? 'Task',
                        deadline: t.deadline ? new Date(t.deadline).toISOString().slice(0, 10) : '',
                        status: t.status,
                    }));

                setDueTasks(filtered);
            } catch (e) {
                message.error('Không tải được KPI cá nhân hoặc danh sách task.');
            }

        };

        init();
    }, []);



    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Tổng quan (Employee Dashboard)</h2>
            <Row gutter={[16, 16]}>
                {/* Cột 1: Theo dõi KPI cá nhân */}
                <Col span={12}>
                    <Card title="Theo dõi KPI cá nhân (Tháng này)" headStyle={{ fontWeight: 'bold' }} style={{ minHeight: '300px' }}>
                        <div style={{ marginBottom: 16 }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                <span>Tiến độ (40%):</span>
                                <span>{kpi.timeliness}%</span>
                            </div>
                            <Progress percent={kpi.timeliness} showInfo={false} status="active" />
                        </div>
                        <div style={{ marginBottom: 16 }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                <span>Hiệu suất (30%):</span>
                                <span>{kpi.efficiency}%</span>
                            </div>
                            <Progress percent={kpi.efficiency} showInfo={false} status="active" strokeColor="#52c41a" />
                        </div>
                        <div style={{ marginBottom: 16 }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                <span>Khối lượng (20%):</span>
                                <span>{kpi.capacity}%</span>
                            </div>
                            <Progress percent={kpi.capacity} showInfo={false} status="active" strokeColor="#faad14" />
                        </div>
                        <div>
                            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                <span>Điểm Quản lý (10%):</span>
                                <span>{kpi.managerScore}%</span>
                            </div>
                            <Progress percent={kpi.managerScore} showInfo={false} status="active" strokeColor="#722ed1" />
                        </div>
                    </Card>
                </Col>

                {/* Cột 2: Công việc sắp đến hạn */}
                <Col span={12}>
                    <Card title="Công việc sắp đến hạn (Due Soon)" headStyle={{ fontWeight: 'bold' }} style={{ minHeight: '300px' }}>
                        <List
                            itemLayout="horizontal"
                            dataSource={dueTasks}
                            renderItem={item => (
                                <List.Item>
                                    <List.Item.Meta
                                        title={<span style={{ fontWeight: 500 }}>{item.name}</span>}
                                        description={
                                            <Space style={{ marginTop: 8 }}>
                                                <Tag icon={<ClockCircleOutlined />}>{item.deadline}</Tag>
                                                <Tag color={item.status === 'Doing' ? 'blue' : 'orange'}>
                                                    {item.status === 'Doing' ? 'Đang làm' : 'Cần làm'}
                                                </Tag>
                                            </Space>
                                        }
                                    />
                                </List.Item>
                            )}
                        />
                    </Card>
                </Col>
            </Row>
        </div>
    );
}
