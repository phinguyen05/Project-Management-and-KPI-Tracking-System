import React, { useState, useEffect } from 'react';
import { Card, Row, Col, List, Tag, Typography, Progress, Button, Space, Badge, Tooltip, Alert } from 'antd';
import { WarningOutlined, UserOutlined, ProjectOutlined, CalendarOutlined, FireOutlined } from '@ant-design/icons';
import api from '../../services/api';

const { Title, Text } = Typography;

export default function ManagerDashboard() {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [overloadEmployees, setOverloadEmployees] = useState([]);
    const [riskTasks, setRiskTasks] = useState([]);
    const [projectsOverview, setProjectsOverview] = useState([]);


    const [ganttTasks, setGanttTasks] = useState([]);
    const [heatmapData, setHeatmapData] = useState([]);

useEffect(() => {
        const fetchDashboardData = async () => {
            setIsLoading(true);
            setError(null);
            try {
                // Làm dynamic theo backend hiện có.
                // Nếu backend chưa có endpoint thống kê Gantt/Heatmap riêng, ta để rỗng và hiển thị text.

                // Lấy WBS (tạm dùng cho mục “Task có nguy cơ trễ hạn”). Backend sẽ tự lọc theo user/role.
                const projectsRes = await api.get("/projects");

                const projects = Array.isArray(projectsRes?.data) ? projectsRes.data : [];
                setProjectsOverview(
                    projects.map((p, idx) => ({
                        id: p.projectId ?? p.id ?? idx + 1,
                        name: p.name ?? p.projectName ?? 'Project',
                        // Backend hiện chưa có progress field chuẩn => tính từ WBS (Leaf tasks)
                        progress: 0,
                    }))
                );

                // Tính tiến độ dự án theo công thức:
                // (Số lượng Task Con có status Done / Tổng số Task Con Leaf) * 100
                // + Lọc leaf task: task không ai khác dùng làm parentTaskId.
                //
                // NOTE: Endpoint WBS hiện tại: GET /tasks/project/{projectId}/wbs
                // Trong thực tế API trả về cây subTasks.
                const computeProjectProgress = async (projectList) => {
                    const results = await Promise.all(
                        projectList.map(async (p) => {
                            try {
                                const wbsRes = await api.get(`/tasks/project/${p.id}/wbs`);
                                const tree = Array.isArray(wbsRes?.data) ? wbsRes.data : [];

                                const flat = [];
                                const flatten = (items) => {
                                    items.forEach((item) => {
                                        if (item) flat.push(item);
                                        const subs = Array.isArray(item?.subTasks) ? item.subTasks : [];
                                        if (subs.length > 0) flatten(subs);
                                    });
                                };
                                flatten(tree);

                                const leaf = flat.filter(
                                    (t) => !flat.some((candidate) => candidate?.parentTaskId === t.taskId)
                                );

                                const totalLeaf = leaf.length;
                                const doneLeaf = leaf.filter((t) => t?.status === 'Done').length;
                                const progress = totalLeaf > 0 ? (doneLeaf / totalLeaf) * 100 : 0;

                                return { ...p, progress: Math.round(progress * 10) / 10 };
                            } catch {
                                return { ...p, progress: 0 };
                            }
                        })
                    );

                    return results;
                };

                const projectsWithProgress = await computeProjectProgress(
                    projects.map((p, idx) => ({
                        id: p.projectId ?? p.id ?? idx + 1,
                        name: p.name ?? p.projectName ?? 'Project',
                        progress: 0,
                    }))
                );

                setProjectsOverview(projectsWithProgress);

                setOverloadEmployees([]);
                setRiskTasks([]);

                // Gantt & Heatmap chưa có endpoint thống kê riêng => để rỗng
                // --- Gọi API thật cho Gantt & Heatmap ---
                // Gantt: cần projectId, ta chọn project đầu tiên (nếu muốn đa dự án có thể mở rộng)
                const firstProjectId = (projectsRes?.data?.[0]?.projectId ?? projectsRes?.data?.[0]?.id) || (projects?.[0]?.projectId ?? projects?.[0]?.id);

                const [ganttRes, heatmapRes] = await Promise.all([
                    firstProjectId ? api.get(`/dashboard/gantt/${firstProjectId}`) : Promise.resolve({ data: [] }),
                    api.get('/dashboard/heatmap').catch(() => ({ data: [] })),
                ]);

                setGanttTasks(Array.isArray(ganttRes?.data) ? ganttRes.data : []);
                setHeatmapData(Array.isArray(heatmapRes?.data) ? heatmapRes.data : []);

            } catch (err) {
                setError(err);
                setOverloadEmployees([]);
                setRiskTasks([]);
                setProjectsOverview([]);
                setGanttTasks([]);
                setHeatmapData([]);
            } finally {
                setIsLoading(false);
            }
        };
        fetchDashboardData();
    }, []);



    const getHeatmapColor = (value) => {
        const v = Number(value);
        if (Number.isNaN(v)) return '#bae7ff';
        if (v > 110) return '#ff4d4f'; // Đỏ
        if (v >= 100 && v <= 110) return '#ffc069'; // Vàng
        return '#bae7ff'; // Xanh
    };

    return (
        <div>
            <Title level={3} style={{ marginBottom: 24 }}>Manager Dashboard</Title>

            {isLoading && (
                <Alert message="Loading..." type="info" showIcon style={{ marginBottom: 16 }} />
            )}
            {!isLoading && error && (
                <Alert
                    message="Không tải được dữ liệu dashboard"
                    description={typeof error?.message === 'string' ? error.message : 'Vui lòng thử lại sau.'}
                    type="error"
                    showIcon
                    style={{ marginBottom: 16 }}
                />
            )}

            <Row gutter={16} style={{ marginBottom: 24 }}>

                <Col xs={24} lg={12}>
                    <Card title={<strong><WarningOutlined /> Cảnh báo Rủi ro & Quá tải</strong>} loading={isLoading} style={{ borderRadius: 8, height: '100%' }}>
                        <Row gutter={16}>
                            <Col span={12}>
                                <Title level={5}><FireOutlined /> Nhân sự quá tải</Title>
                                <List
                                    dataSource={overloadEmployees}
                                    renderItem={(item) => (
                                        <List.Item>
                                            <div style={{ width: '100%' }}>
                                                <Text strong style={{ display: 'block' }}>{item.name}</Text>
                                                <Tag color="red">{item.overload_percentage}%</Tag>
                                                <Text type="danger" style={{ fontSize: '12px' }}> {item.action_needed} khẩn cấp</Text>
                                            </div>
                                        </List.Item>
                                    )}
                                />
                            </Col>
                            <Col span={12}>
                                <Title level={5}>Task có nguy cơ trễ hạn</Title>
                                {Array.isArray(riskTasks) && riskTasks.length > 0 ? (
                                    <List
                                        dataSource={riskTasks}
                                        renderItem={(item) => (
                                            <List.Item>
                                                <div style={{ width: '100%' }}>
                                                    <Text strong style={{ display: 'block' }}>{item.name}</Text>
                                                    <Space style={{ marginTop: 4 }}>
                                                        <Tag color="orange">{item.deadline}</Tag>
                                                        <Text type="secondary" style={{ fontSize: '12px' }}>({item.assignee})</Text>
                                                    </Space>
                                                </div>
                                            </List.Item>
                                        )}
                                    />
                                ) : null}

                            </Col>
                        </Row>
                    </Card>
                </Col>

                <Col xs={24} lg={12}>
                    <Card title={<strong><ProjectOutlined /> Tiến độ dự án tổng thể</strong>} loading={isLoading} style={{ borderRadius: 8, height: '100%' }}>
                        <Title level={5} style={{ color: '#555' }}><CalendarOutlined /> Kỳ đánh giá: Tháng 06/2026</Title>
                        <List
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
                                        strokeColor={item.progress < 50 ? '#ff4d4f' : (item.progress < 80 ? '#faad14' : '#52c41a')}
                                    />
                                </List.Item>
                            )}
                        />
                    </Card>
                </Col>
            </Row>

            <Card title={<strong>Sơ đồ Gantt tiến độ dự án (Dependencies: Finish-to-Start)</strong>} style={{ marginBottom: 24, borderRadius: 8 }}>
                <div style={{ overflowX: 'auto', padding: '10px 0' }}>
                    <div style={{ minWidth: 600 }}>
                        <Row style={{ background: '#f5f5f5', padding: '8px 16px', fontWeight: 'bold', borderBottom: '1px solid #e8e8e8' }}>
                            <Col span={8}>Tên gói công việc (Task)</Col>
                            <Col span={4}>Thời gian</Col>
                            <Col span={4}>Phụ thuộc</Col>
                            <Col span={8}>Tiến độ</Col>
                        </Row>
                        {Array.isArray(ganttTasks) && ganttTasks.length > 0 ? (
                            ganttTasks.map((task) => (

                                <Row key={task.id} style={{ padding: '12px 16px', borderBottom: '1px solid #f0f0f0', alignItems: 'center' }}>
                                    <Col span={8}><Text strong>{task.name}</Text></Col>
                                    <Col span={4}><Text type="secondary">{task.start} - {task.end}</Text></Col>
                                    <Col span={4}><Tag>{task.dependency}</Tag></Col>
                                    <Col span={8}>
                                        <div style={{ background: '#f0f2f5', height: 24, borderRadius: 12, position: 'relative', overflow: 'hidden' }}>
                                            <div style={{
                                                position: 'absolute',
                                                left: task.id === 1 ? '5%' : (task.id === 2 ? '35%' : '65%'),
                                                width: '30%',
                                                height: '100%',
                                                background: task.progress === 100 ? '#52c41a' : '#1890ff',
                                                borderRadius: 12,
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                color: '#fff',
                                                fontSize: '11px',
                                                fontWeight: 'bold'
                                            }}>
                                                {task.progress}%
                                            </div>
                                        </div>
                                    </Col>
                                </Row>
                            ))
                        ) : (
                            <Text type="secondary">Chưa có dữ liệu.</Text>
                        )}
                    </div>
                </div>
            </Card>

            <Card title={<strong>Bản đồ nhiệt tải lượng nhân sự (Overload Heatmap)</strong>} style={{ borderRadius: 8 }}>
                <Alert
                    message="Quy tắc điều chuyển công việc"
                    description="Các ô màu đỏ rực tượng trưng cho nhân sự bị quá tải công việc ngày hôm đó (>110%). Manager cần kéo thả hoặc phân phối lại Task để giảm tải lượng về vùng màu Xanh nhạt."
                    type="warning"
                    showIcon
                    style={{ marginBottom: 20 }}
                />

                <div style={{ overflowX: 'auto' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'center' }}>
                        <thead>
                            <tr style={{ background: '#fafafa', height: 40, borderBottom: '2px solid #f0f0f0' }}>
                                <th style={{ padding: 10, width: '15%' }}>Nhân sự</th>
                                <th style={{ padding: 10 }}>Thứ 2</th>
                                <th style={{ padding: 10 }}>Thứ 3</th>
                                <th style={{ padding: 10 }}>Thứ 4</th>
                                <th style={{ padding: 10 }}>Thứ 5</th>
                                <th style={{ padding: 10 }}>Thứ 6</th>
                                <th style={{ padding: 10, width: '15%' }}>Trạng thái tuần</th>
                            </tr>
                        </thead>
                        <tbody>
                            {Array.isArray(heatmapData) && heatmapData.length > 0 ? (
                                heatmapData.map((row, index) => (
                                    <tr key={index} style={{ borderBottom: '1px solid #f0f0f0', height: 50 }}>
                                        <td style={{ fontWeight: 'bold', padding: 10 }}>{row.name}</td>
                                        <td style={{ background: getHeatmapColor(row.mon), color: row.mon >= 110 ? '#fff' : '#000', fontWeight: 'bold' }}>{row.mon}%</td>
                                        <td style={{ background: getHeatmapColor(row.tue), color: row.tue >= 110 ? '#fff' : '#000', fontWeight: 'bold' }}>{row.tue}%</td>
                                        <td style={{ background: getHeatmapColor(row.wed), color: row.wed >= 110 ? '#fff' : '#000', fontWeight: 'bold' }}>{row.wed}%</td>
                                        <td style={{ background: getHeatmapColor(row.thu), color: row.thu >= 110 ? '#fff' : '#000', fontWeight: 'bold' }}>{row.thu}%</td>
                                        <td style={{ background: getHeatmapColor(row.fri), color: row.fri >= 110 ? '#fff' : '#000', fontWeight: 'bold' }}>{row.fri}%</td>
                                        <td style={{ padding: 10 }}>
                                            <Tag color={row.status === 'Overload' ? 'red' : 'green'}>{row.status}</Tag>
                                        </td>
                                    </tr>
                                ))
                            ) : null}

                        </tbody>
                    </table>
                </div>
            </Card>
        </div>
    );
}
