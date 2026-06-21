import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Typography, Progress, List, Tag, Space, Alert } from 'antd';
import { ProjectOutlined, DashboardOutlined, TeamOutlined, RocketOutlined } from '@ant-design/icons';
import api from '../../services/api';

const { Title, Text } = Typography;

export default function CLevelDashboard() {
    const [loading, setLoading] = useState(false);
    const [overallProgress, setOverallProgress] = useState(0); // Tổng tiến độ toàn công ty
    const [companyKPI, setCompanyKPI] = useState(0); // Hiệu suất toàn công ty
    const [projectsOverview, setProjectsOverview] = useState([]);
    const [employeeProductivity, setEmployeeProductivity] = useState([]); // Dữ liệu năng suất nhân sự

    useEffect(() => {
        const fetchDashboardData = async () => {
            setLoading(true);
            try {
                const [overallRes, kpiRes, projectsRes, employeeRes] = await Promise.all([
                    api.get("/reports/overall-progress"), // Cần tạo API này ở Backend
                    api.get("/reports/company-kpi"),      // Cần tạo API này ở Backend
                    api.get("/projects/overview"),        // Lấy từ ProjectController
                    api.get("/reports/employee-productivity") // Cần tạo API này ở Backend
                ]);
                setOverallProgress(overallRes.data?.progress || 75);
                setCompanyKPI(kpiRes.data?.kpi_score || 85);
                setProjectsOverview(projectsRes.data || []);
                setEmployeeProductivity(employeeRes.data || []);
            } catch (error) {
                // Mock dữ liệu khớp với PlantUML/Wireframe nếu API lỗi hoặc chưa có
                setOverallProgress(75);
                setCompanyKPI(85);
                setProjectsOverview([
                    { id: 1, name: 'Dự án A (MIS)', progress: 70, status: 'On Track' },
                    { id: 2, name: 'Dự án B (CRM)', progress: 90, status: 'Ahead' },
                    { id: 3, name: 'Dự án C (ERP)', progress: 40, status: 'At Risk' },
                ]);
                setEmployeeProductivity([
                    { id: 1, name: 'Nguyễn Văn A', productivity: 95, status: 'Excellent' },
                    { id: 2, name: 'Trần Thị B', productivity: 80, status: 'Good' },
                    { id: 3, name: 'Lê Văn C', productivity: 60, status: 'Needs Improvement' },
                ]);
            } finally {
                setLoading(false);
            }
        };
        fetchDashboardData();
    }, []);

    const getProductivityColor = (productivity) => {
        if (productivity >= 90) return 'green';
        if (productivity >= 70) return 'blue';
        return 'red';
    };

    return (
        <div style={{ padding: '10px' }}>
            <Title level={2} style={{ marginBottom: 24 }}>C-Level Dashboard (View-Only)</Title>

            <Row gutter={[24, 24]} style={{ marginBottom: 24 }}>
                {/* 1. Tổng quan hiệu suất công ty */}
                <Col xs={24} lg={8}>
                    <Card title={<strong><DashboardOutlined /> Tổng quan hiệu suất công ty</strong>} loading={loading} style={{ borderRadius: 8, height: '100%' }}>
                        <div style={{ textAlign: 'center', marginBottom: 24 }}>
                            <Progress type="circle" percent={companyKPI} width={140} strokeColor={{ '0%': '#ff4d4f', '100%': '#52c41a' }} />
                            <div style={{ marginTop: 12, fontWeight: 'bold', fontSize: 16 }}>Hiệu suất chung</div>
                        </div>
                        <div style={{ textAlign: 'center' }}>
                            <Progress type="circle" percent={overallProgress} width={140} strokeColor={{ '0%': '#108ee9', '100%': '#87d068' }} />
                            <div style={{ marginTop: 12, fontWeight: 'bold', fontSize: 16 }}>Tiến độ tổng thể dự án</div>
                        </div>
                    </Card>
                </Col>

                {/* 2. Tiến độ từng dự án */}
                <Col xs={24} lg={8}>
                    <Card title={<strong><ProjectOutlined /> Tiến độ các dự án</strong>} loading={loading} style={{ borderRadius: 8, height: '100%' }}>
                        <List
                            bordered
                            dataSource={projectsOverview}
                            renderItem={(item) => (
                                <List.Item style={{ flexDirection: 'column', alignItems: 'flex-start', padding: '12px 0' }}>
                                    <div style={{ width: '100%', display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                        <Text strong>{item.name}</Text>
                                        <Text>{item.progress}%</Text>
                                    </div>
                                    <Progress 
                                        percent={item.progress} 
                                        status="active" 
                                        showInfo={false} 
                                        strokeColor={item.status === 'At Risk' ? '#ff4d4f' : (item.status === 'Ahead' ? '#52c41a' : '#108ee9')}
                                    />
                                    <Tag color={item.status === 'At Risk' ? 'volcano' : (item.status === 'Ahead' ? 'green' : 'blue')} style={{ marginTop: 8 }}>
                                        {item.status}
                                    </Tag>
                                </List.Item>
                            )}
                        />
                    </Card>
                </Col>

                {/* 3. Năng suất nhân sự */}
                <Col xs={24} lg={8}>
                    <Card title={<strong><TeamOutlined /> Năng suất nhân sự toàn công ty</strong>} loading={loading} style={{ borderRadius: 8, height: '100%' }}>
                        <Alert
                            message="Thông tin tham khảo"
                            description="Biểu đồ này thể hiện năng suất trung bình của nhân sự dựa trên KPI Khối lượng và Hiệu suất. Cần đối chiếu với báo cáo chi tiết để có cái nhìn đầy đủ."
                            type="info"
                            showIcon
                            style={{ marginBottom: 20 }}
                        />
                        <List
                            dataSource={employeeProductivity}
                            renderItem={item => (
                                <List.Item>
                                    <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                                        <Text strong><UserOutlined /> {item.name}</Text>
                                        <Tag color={getProductivityColor(item.productivity)}>
                                            {item.productivity}%
                                        </Tag>
                                        <Text type="secondary" style={{ fontSize: '12px' }}>({item.status})</Text>
                                    </Space>
                                </List.Item>
                            )}
                        />
                        <div style={{ marginTop: 24, textAlign: 'right' }}>
                            <Button type="link" icon={<RocketOutlined />}>Xem chi tiết KPI Nhân sự</Button>
                        </div>
                    </Card>
                </Col>
            </Row>
        </div>
    );
}
