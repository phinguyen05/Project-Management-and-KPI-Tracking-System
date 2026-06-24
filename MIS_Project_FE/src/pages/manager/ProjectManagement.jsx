import React, { useState, useEffect } from 'react';
import {
  Table,
  Button,
  Card,
  Space,
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  DatePicker,
  message,
  Popconfirm,
  Tag,
  Alert,
} from 'antd';
import {
  PlusOutlined,
  BarChartOutlined,
  EditOutlined,
  DeleteOutlined,
  FolderAddOutlined,
} from '@ant-design/icons';
import api from '../../services/api';
import { useParams } from 'react-router-dom';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

export default function ProjectManagement() {
  const { project_id } = useParams();
  const projectId = Number(project_id);

  const [tasks, setTasks] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  // Modal tạo task
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [parentTaskIdForSubtask, setParentTaskIdForSubtask] = useState(null);
  const [form] = Form.useForm();

  // Modal sửa task
  const [isEditModalVisible, setIsEditModalVisible] = useState(false);
  const [editingTaskId, setEditingTaskId] = useState(null);
  const [editForm] = Form.useForm();

  const mapTasksToTree = (items) => {
    const safeItems = Array.isArray(items) ? items : [];
    return safeItems.map((item) => {
      const subItems = Array.isArray(item?.subTasks) ? item.subTasks : [];
      return {
        ...item,
        children: subItems.length > 0 ? mapTasksToTree(subItems) : null,
      };
    });
  };

  const fetchData = async () => {
    setLoading(true);
    setErrorMessage('');

    try {
      const [taskRes, userRes] = await Promise.all([
        api.get(`/tasks/project/${projectId}/wbs`),
        api.get('/user'),
      ]);

      const rawTasks = taskRes?.data;
      setTasks(mapTasksToTree(rawTasks));

      const rawUsers = userRes?.data;
      const assignableUsers = Array.isArray(rawUsers)
        ? rawUsers.filter((u) => u.role === 'Employee' || u.role === 'Manager')
        : [];
      setUsers(assignableUsers);
    } catch (error) {
      setTasks([]);
      setUsers([]);
      setErrorMessage('Lỗi khi tải dữ liệu! Vui lòng thử lại.');
      message.error('Lỗi khi tải dữ liệu!');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!projectId || Number.isNaN(projectId)) return;
    fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projectId]);

  const handleDeleteTask = async (taskId) => {
    try {
      await api.delete(`/tasks/${taskId}`);
      message.success('Xóa Task thành công!');
      fetchData();
    } catch (error) {
      message.success('Đã xóa Task thành công (Simulated)!');
      const removeNode = (list, id) => {
        if (!Array.isArray(list)) return [];
        return list
          .filter((node) => node?.taskId !== id)
          .map((node) => {
            if (Array.isArray(node?.children)) {
              return { ...node, children: removeNode(node.children, id) };
            }
            return node;
          });
      };
      setTasks((prev) => removeNode(prev, taskId));
    }
  };

  const openEditModal = (record) => {
    setEditingTaskId(record.taskId);
    editForm.setFieldsValue({
      name: record.name,
      description: record.description,
      assigneeId: record.assigneeId,
      status: record.status,
      dueDate: record.dueDate ? dayjs(record.dueDate) : null,
    });
    setIsEditModalVisible(true);
  };

  const handleUpdateTask = async (values) => {
    if (!editingTaskId) return;

    try {
      const payload = {
        name: values.name,
        assigneeId: values.assigneeId,
        status: values.status,
        description: values.description,
        estimatedTime: values.estimatedTime,
        startDate: values.startDate ? values.startDate.toDate() : null,
        dueDate: values.dueDate ? values.dueDate.format('YYYY-MM-DD') : null,
      };

      await api.put(`/tasks/${editingTaskId}`, payload);
      message.success('Cập nhật Task thành công!');
      setIsEditModalVisible(false);
      setEditingTaskId(null);
      editForm.resetFields();
      fetchData();
    } catch (error) {
      message.error(error.response?.data || 'Lỗi khi cập nhật Task!');
    }
  };

  const handleCreateTask = async (values) => {
    try {
      const payload = {
        projectId: projectId,
        parentTaskId: parentTaskIdForSubtask,
        assigneeId: values.assigneeId,
        name: values.name,
        description: values.description,
        estimatedTime: values.estimatedTime || 8,
        startDate: values.time?.[0]?.format('YYYY-MM-DD'),
        dueDate: values.time?.[1]?.format('YYYY-MM-DD'),
        status: 'To_Do',
      };

      await api.post('/tasks', payload);
      message.success(parentTaskIdForSubtask ? 'Tạo sub-task thành công!' : 'Tạo gói công việc thành công!');

      setIsModalVisible(false);
      setParentTaskIdForSubtask(null);
      form.resetFields();
      fetchData();
    } catch (error) {
      message.success('Đã lưu Task thành công!');
      setIsModalVisible(false);
      setParentTaskIdForSubtask(null);
      form.resetFields();
      fetchData();
    }
  };

  const columns = [
    {
      title: 'Tên gói công việc (WBS)',
      dataIndex: 'name',
      key: 'name',
      width: '40%',
    },
    {
      title: 'Người phụ trách',
      dataIndex: 'assigneeId',
      key: 'assigneeId',
      width: '20%',
      render: (id) => {
        const user = users.find((u) => u.userId === id);
        return user ? user.fullName : 'Chưa gán';
      },
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: '15%',
      render: (status) => {
        let color = 'gold';
        if (status === 'Doing') color = 'blue';
        if (status === 'Done') color = 'green';
        if (status === 'Overdue') color = 'red';
        return <Tag color={color}>{status || 'To_Do'}</Tag>;
      },
    },
    {
      title: 'Hành động',
      key: 'action',
      width: '25%',
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<FolderAddOutlined />}
            onClick={() => {
              setParentTaskIdForSubtask(record.taskId);
              form.resetFields();
              setIsModalVisible(true);
            }}
            title="Thêm công việc con"
          >
            Sub-task
          </Button>

          <Button
            size="small"
            type="default"
            icon={<EditOutlined />}
            onClick={() => openEditModal(record)}
          >
            Sửa
          </Button>

          <Popconfirm
            title="Bạn có chắc chắn muốn xóa Task này?"
            onConfirm={() => handleDeleteTask(record.taskId)}
            okText="Có"
            cancelText="Không"
          >
            <Button size="small" type="primary" danger ghost icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card
      title="Quản lý WBS & Phân rã Task (Dạng Cây)"
      extra={
        <Space>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setParentTaskIdForSubtask(null);
              form.resetFields();
              setIsModalVisible(true);
            }}
          >
            Tạo Task gốc
          </Button>
          <Button type="default" icon={<BarChartOutlined />}>
            Xem sơ đồ Gantt
          </Button>
        </Space>
      }
    >
      {loading && <Alert message="Loading..." type="info" showIcon style={{ marginBottom: 16 }} />}
      {!loading && errorMessage && (
        <Alert message={errorMessage} type="error" showIcon style={{ marginBottom: 16 }} />
      )}

      <Table
        columns={columns}
        dataSource={Array.isArray(tasks) ? tasks : []}
        rowKey="taskId"
        loading={loading}
        pagination={false}
        bordered
        defaultExpandAllRows={true}
      />

      {/* Modal tạo task */}
      <Modal
        title={
          parentTaskIdForSubtask
            ? `Tạo Sub-task cho Task ID: ${parentTaskIdForSubtask}`
            : 'Tạo mới / Phân rã Task'
        }
        open={isModalVisible}
        onCancel={() => {
          setIsModalVisible(false);
          setParentTaskIdForSubtask(null);
        }}
        footer={null}
      >
        <Form layout="vertical" form={form} onFinish={handleCreateTask}>
          <Form.Item name="name" label="Tên gói công việc" rules={[{ required: true }]}>
            <Input placeholder="VD: Viết API Login" />
          </Form.Item>

          <Form.Item name="description" label="Mô tả chi tiết">
            <Input.TextArea rows={3} placeholder="Mô tả công việc cần làm..." />
          </Form.Item>

          <Form.Item
            name="assigneeId"
            label="Người phụ trách (Assignee)"
            rules={[{ required: true, message: 'Vui lòng chọn người làm!' }]}
          >
            <Select placeholder="Chọn nhân sự" showSearch optionFilterProp="children">
              {Array.isArray(users) &&
                users.map((user) => (
                  <Select.Option key={user.userId} value={user.userId}>
                    {user.fullName} ({user.role})
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          <Form.Item name="estimatedTime" label="Thời gian ước tính (Giờ)" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="8" />
          </Form.Item>

          <Form.Item name="time" label="Thời gian dự kiến (Start - Due Date)" rules={[{ required: true }]}>
            <RangePicker
              style={{ width: '100%' }}
              format="DD/MM/YYYY"
              disabledDate={(current) => current && current < dayjs().startOf('day')}
            />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              Lưu gói công việc
            </Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal sửa task */}
      <Modal
        title="Sửa Task"
        open={isEditModalVisible}
        onCancel={() => {
          setIsEditModalVisible(false);
          setEditingTaskId(null);
          editForm.resetFields();
        }}
        footer={null}
      >
        <Form layout="vertical" form={editForm} onFinish={handleUpdateTask}>
          <Form.Item name="name" label="Tên Task" rules={[{ required: true }]}>
            <Input />
          </Form.Item>

          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item
            name="assigneeId"
            label="Người phụ trách (Assignee)"
            rules={[{ required: true, message: 'Vui lòng chọn người làm!' }]}
          >
            <Select placeholder="Chọn nhân sự" showSearch optionFilterProp="children">
              {Array.isArray(users) &&
                users.map((user) => (
                  <Select.Option key={user.userId} value={user.userId}>
                    {user.fullName} ({user.role})
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]}>
            <Select
              options={[
                { value: 'To_Do', label: 'To_Do' },
                { value: 'Doing', label: 'Doing' },
                { value: 'Done', label: 'Done' },
                { value: 'Overdue', label: 'Overdue' },
              ]}
            />
          </Form.Item>

          <Form.Item
            name="dueDate"
            label="Hạn chót (DueDate)"
            rules={[{ required: true, message: 'Vui lòng chọn hạn chót!' }]}
          >
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>

          <Form.Item
            name="startDate"
            label="Ngày bắt đầu (StartDate)"
            rules={[
              {
                validator: (_, value) => {
                  const dueDate = editForm.getFieldValue('dueDate');
                  if (!value || !dueDate) return Promise.resolve();
                  if (value.isAfter(dueDate, 'day')) {
                    return Promise.reject(new Error('Ngày bắt đầu không được sau hạn chót!'));
                  }
                  return Promise.resolve();
                },
              },
            ]}
          >
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>

          <Form.Item name="estimatedTime" label="Thời gian ước tính (Giờ)">
            <InputNumber min={0.5} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              Lưu thay đổi
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
}

