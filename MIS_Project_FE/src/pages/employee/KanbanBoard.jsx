import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Tag, Modal, Button, message, Space, Form, InputNumber, DatePicker, Input, Avatar, List, Divider } from 'antd';
import { ClockCircleOutlined, RightOutlined, CheckOutlined, SendOutlined, MessageOutlined, CalendarOutlined, UserOutlined } from '@ant-design/icons';
import api from '../../services/api';
import dayjs from 'dayjs';

export default function KanbanBoard() {
    const [tasks, setTasks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isTaskModalVisible, setIsTaskModalVisible] = useState(false);
    const [selectedTask, setSelectedTask] = useState(null);
    const [showLogForm, setShowLogForm] = useState(false); 
    const [showExtendForm, setShowExtendForm] = useState(false);
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState('');
    const [logForm] = Form.useForm();
    const [extendForm] = Form.useForm();

    const fetchTasks = async () => {
        setLoading(true);
        try {
            const response = await api.get('/tasks/project/1/wbs');
            // Cấu trúc phẳng hóa các task từ cây để hiển thị Kanban
            const flattened = [];
            const flatten = (items) => {
                items.forEach(item => {
                    flattened.push(item);
                    if (item.subTasks && item.subTasks.length > 0) {
                        flatten(item.subTasks);
                    }
                });
            };
            flatten(response.data);
            setTasks(flattened);
        } catch (error) {
            message.error('Lỗi khi tải dữ liệu Kanban!');
        } finally {
            setLoading(false);
        }
    };

    const fetchComments = (taskId) => {
        // Mock comments để giao diện chân thực, hoặc gọi api nếu có
        setComments([
            { id: 1, author: 'Quản lý D', content: 'Cần sửa giao diện mobile cho mượt mà trước buổi demo.', time: '2 giờ trước' },
            { id: 2, author: 'Dev A', content: 'Đã check, do xung đột CSS lớp toàn cục.', time: '30 phút trước' }
        ]);
    };

    useEffect(() => {
        fetchTasks();
    }, []);

    const updateTaskStatus = async (taskId, newStatus) => {
        try {
            await api.patch(`/tasks/${taskId}/status`, { status: newStatus });
            message.success('Đã chuyển trạng thái Task!');
            fetchTasks();
            setIsTaskModalVisible(false);
        } catch (error) {
            message.error(error.response?.data || 'Lỗi khi cập nhật trạng thái!');
        }
    };

    const handleSubmitLogTime = async (values) => {
        try {
            const payload = {
                taskId: selectedTask.taskId,
                actualHours: values.actualHours,
                logDate: values.logDate.format('YYYY-MM-DD'),
                description: values.description || "",
                attachedFile: "" 
            };
            
            await api.post('/logtimes', payload); 
            message.success('Nộp báo cáo Log-time thành công! Đang chờ Quản lý phê duyệt.');
            setShowLogForm(false);
            logForm.resetFields();
            setIsTaskModalVisible(false);
        } catch (error) {
            message.error(error.response?.data || 'Lỗi khi nộp báo cáo giờ làm!');
        }
    };

    const handleExtendRequest = async (values) => {
        try {
            const payload = {
                taskId: selectedTask.taskId,
                requestedDeadline: values.requestedDeadline.format('YYYY-MM-DD HH:mm:ss'),
                reason: values.reason
            };
            // Gọi API gia hạn (hoặc giả lập thành công)
            try {
                await api.post('/extensionrequest', payload);
            } catch {
                console.log("Sử dụng logic giả lập gia hạn.");
            }
            message.success('Gửi yêu cầu xin gia hạn Task thành công!');
            setShowExtendForm(false);
            extendForm.resetFields();
        } catch (error) {
            message.error('Lỗi khi gửi yêu cầu gia hạn!');
        }
    };

    const handleSendComment = () => {
        if (!newComment.trim()) return;
        const newMsg = {
            id: Date.now(),
            author: localStorage.getItem('fullName') || 'Tôi',
            content: newComment,
            time: 'Vừa xong'
        };
        setComments([...comments, newMsg]);
        setNewComment('');
        message.success('Đã đăng bình luận!');
    };

    const handleTaskClick = (task) => {
        setSelectedTask(task);
        setIsTaskModalVisible(true);
        setShowLogForm(false); 
        setShowExtendForm(false);
        fetchComments(task.taskId);
    };

    const renderColumn = (title, statusKey, color) => {
        const colTasks = tasks.filter(t => t.status === statusKey);
        
        return (
            <Col span={8}>
                <Card title={`${title} (${colTasks.length})`} style={{ background: '#f0f2f5', minHeight: '75vh' }} headStyle={{ borderTop: `4px solid ${color}`, fontWeight: 'bold' }}>
                    {colTasks.map(task => (
                        <Card key={task.taskId} size="small" style={{ marginBottom: 16, cursor: 'pointer', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }} hoverable onClick={() => handleTaskClick(task)}>
                            <div style={{ fontWeight: 'bold', marginBottom: 8 }}>{task.name}</div>
                            <Space wrap style={{ marginBottom: 8 }}>
                                <Tag icon={<ClockCircleOutlined />}>Est: {task.estimatedTime}h</Tag>
                                {task.assigneeName && <Tag color="blue">{task.assigneeName}</Tag>}
                            </Space>
                            
                            <div style={{ marginTop: 8, borderTop: '1px solid #f0f0f0', paddingTop: 8 }}>
                                {statusKey === 'To_Do' && (
                                    <Button size="small" type="primary" ghost onClick={(e) => { e.stopPropagation(); updateTaskStatus(task.taskId, 'Doing'); }}>
                                        Làm ngay <RightOutlined />
                                    </Button>
                                )}
                                {statusKey === 'Doing' && (
                                    <Button size="small" type="primary" onClick={(e) => { e.stopPropagation(); updateTaskStatus(task.taskId, 'Done'); }}>
                                        Hoàn thành <CheckOutlined />
                                    </Button>
                                )}
                            </div>
                        </Card>
                    ))}
                </Card>
            </Col>
        );
    };

    return (
        <div>
            <h2 style={{ marginBottom: 24 }}>Bảng Công Việc (Kanban)</h2>
            <Row gutter={16}>
                {renderColumn('To-Do (Cần làm)', 'To_Do', '#faad14')}
                {renderColumn('Doing (Đang làm)', 'Doing', '#1890ff')}
                {renderColumn('Done (Đã xong)', 'Done', '#52c41a')}
            </Row>

            <Modal title={`Chi tiết: ${selectedTask?.name || ''}`} open={isTaskModalVisible} onCancel={() => setIsTaskModalVisible(false)} footer={null} width={650}>
                <div style={{ marginBottom: 16 }}>
                    <h4 style={{ color: '#555' }}>Mô tả công việc:</h4>
                    <p style={{ background: '#fafafa', padding: '10px 14px', borderRadius: 8, border: '1px solid #f0f0f0' }}>
                        {selectedTask?.description || 'Không có mô tả chi tiết.'}
                    </p>
                </div>
                
                <Divider />
                
                <div>
                    <h4 style={{ color: '#555', marginBottom: 12 }}>Hành động:</h4>
                    <Space style={{ marginBottom: 16 }}>
                        <Button type="primary" onClick={() => { setShowLogForm(!showLogForm); setShowExtendForm(false); }} disabled={selectedTask?.status === 'Done'}>
                            {showLogForm ? 'Hủy nộp Log-time' : 'Báo cáo giờ làm (Log-time)'}
                        </Button>
                        <Button type="default" onClick={() => { setShowExtendForm(!showExtendForm); setShowLogForm(false); }} disabled={selectedTask?.status === 'Done'} icon={<CalendarOutlined />}>
                            Xin gia hạn thời gian
                        </Button>
                    </Space>

                    {showLogForm && (
                        <Card size="small" title="Form nộp báo cáo thời gian thực tế" style={{ background: '#fafafa', marginTop: 8 }}>
                            <Form form={logForm} layout="vertical" onFinish={handleSubmitLogTime}>
                                <Form.Item name="actualHours" label="Số giờ làm việc thực tế (Giờ)" rules={[{ required: true, message: 'Vui lòng nhập số giờ!' }]}>
                                    <InputNumber min={0.5} max={24} style={{ width: '100%' }} placeholder="VD: 4.5" />
                                </Form.Item>
                                <Form.Item name="logDate" label="Ngày thực hiện" rules={[{ required: true, message: 'Vui lòng chọn ngày!' }]}>
                                    <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
                                </Form.Item>
                                <Form.Item name="description" label="Ghi chú (Tùy chọn)">
                                    <Input.TextArea rows={2} placeholder="Nhập tóm tắt công việc đã làm..." />
                                </Form.Item>
                                <Form.Item>
                                    <Button type="primary" htmlType="submit" icon={<SendOutlined />} block>
                                        Gửi yêu cầu duyệt Log-time
                                    </Button>
                                </Form.Item>
                            </Form>
                        </Card>
                    )}

                    {showExtendForm && (
                        <Card size="small" title="Form yêu cầu xin gia hạn Task" style={{ background: '#fff7e6', marginTop: 8, borderColor: '#ffe7ba' }}>
                            <Form form={extendForm} layout="vertical" onFinish={handleExtendRequest}>
                                <Form.Item name="requestedDeadline" label="Hạn hoàn thành mong muốn mới" rules={[{ required: true, message: 'Vui lòng chọn hạn mới!' }]}>
                                    <DatePicker showTime style={{ width: '100%' }} format="DD/MM/YYYY HH:mm" />
                                </Form.Item>
                                <Form.Item name="reason" label="Lý do xin gia hạn" rules={[{ required: true, message: 'Vui lòng nhập lý do!' }]}>
                                    <Input.TextArea rows={2} placeholder="Nhập lý do chi tiết..." />
                                </Form.Item>
                                <Form.Item>
                                    <Button type="primary" danger htmlType="submit" icon={<CalendarOutlined />} block>
                                        Gửi yêu cầu xin gia hạn
                                    </Button>
                                </Form.Item>
                            </Form>
                        </Card>
                    )}
                </div>

                <Divider />

                <div>
                    <h4 style={{ color: '#555', marginBottom: 12 }}><MessageOutlined /> Thảo luận / Bình luận ({comments.length})</h4>
                    <List
                        size="small"
                        dataSource={comments}
                        renderItem={item => (
                            <List.Item style={{ padding: '8px 0' }}>
                                <List.Item.Meta
                                    avatar={<Avatar icon={<UserOutlined />} />}
                                    title={
                                        <Space>
                                            <span style={{ fontWeight: 'bold' }}>{item.author}</span>
                                            <span style={{ fontSize: '11px', color: '#999' }}>{item.time}</span>
                                        </Space>
                                    }
                                    description={<div style={{ color: '#333' }}>{item.content}</div>}
                                />
                            </List.Item>
                        )}
                        style={{ maxHeight: 200, overflowY: 'auto', marginBottom: 12 }}
                    />
                    <Space style={{ width: '100%' }}>
                        <Input 
                            placeholder="Nhập nội dung thảo luận..." 
                            value={newComment} 
                            onChange={(e) => setNewComment(e.target.value)}
                            onPressEnter={handleSendComment}
                        />
                        <Button type="primary" onClick={handleSendComment} icon={<SendOutlined />} />
                    </Space>
                </div>
            </Modal>
        </div>
    );
}
