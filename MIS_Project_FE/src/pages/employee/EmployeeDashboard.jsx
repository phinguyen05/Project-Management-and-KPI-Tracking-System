import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Progress, List, Tag, Space } from 'antd'; // Đã import đầy đủ Space và Progress
import { ClockCircleOutlined } from '@ant-design/icons';

export default function EmployeeDashboard() {
    const [kpi, setKpi] = useState({ timeliness: 0, efficiency: 0, capacity: 0, managerScore: 0 });
    const [dueTasks, setDueTasks] = useState([]);

    useEffect(() => {
        // Tạm thời dùng dữ liệu giả để tránh lỗi 404 API Not Found
        // Chờ đến khi Module KPI Backend hoàn thiện sẽ nối API thật vào đây
        setKpi({ timeliness: 90, efficiency: 70, capacity: 100, managerScore: 40 });
        setDueTasks([
            { id: 1, name: 'Fix lỗi UI bảng Gantt', deadline: '2026-06-20', status: 'Doing' },
            { id: 2, name: 'Viết tài liệu API', deadline: '2026-06-21', status: 'To_Do' }
        ]);
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
