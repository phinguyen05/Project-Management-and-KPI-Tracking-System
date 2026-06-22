import React, { useEffect, useMemo, useState } from 'react';
import {
  Table,
  Button,
  Card,
  Space,
  Modal,
  Form,
  Input,
  DatePicker,
  message,
  Tag,
  Alert,
  Popconfirm,
} from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import api from '../../services/api';

const { RangePicker } = DatePicker;

export default function ProjectList() {
  const navigate = useNavigate();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const [isModalVisible, setIsModalVisible] = useState(false);
  const [form] = Form.useForm();

  const token = useMemo(() => localStorage.getItem('token'), []);

  // Backend yêu cầu CreateProjectDto có ManagerId (int).
  // FE hiện đang chỉ lưu token + role, nên ta lấy userId/managerId từ claim trong JWT (nếu có).
  const getManagerIdFromToken = () => {
    try {
      const decoded = jwtDecode(token);

      // Common claim patterns
      const nameIdentifier = decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      const nameId2 = decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier'];
      const sub = decoded?.sub;
      const managerId = decoded?.managerId ?? decoded?.ManagerId;
      const userId = decoded?.userId ?? decoded?.UserId;

      const candidate = managerId ?? userId ?? nameIdentifier ?? nameId2 ?? sub;
      const idNum = candidate !== undefined && candidate !== null ? Number(candidate) : NaN;
      return Number.isFinite(idNum) ? idNum : null;
    } catch {
      return null;
    }
  };

  const managerIdFromToken = useMemo(() => {
    if (!token) return null;
    return getManagerIdFromToken();
  }, [token]);

  const fetchProjects = async () => {
    setLoading(true);
    setErrorMessage('');
    try {
      const res = await api.get('/projects');
      setProjects(Array.isArray(res?.data) ? res.data : []);
    } catch (e) {
      setProjects([]);
      setErrorMessage('Lỗi khi tải danh sách dự án! Vui lòng thử lại.');
      message.error('Lỗi khi tải danh sách dự án!');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProjects();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleOpenCreateProject = () => {
    if (!managerIdFromToken) {
      message.error(
        'Không lấy được ManagerId từ token hiện tại. Hãy kiểm tra backend JWT claims hoặc bổ sung lưu userId/managerId trong FE.'
      );
      return;
    }

    form.resetFields();
    setIsModalVisible(true);
  };

  const handleCreateProject = async (values) => {
    try {
      const { name, description, time } = values;
      const startDate = time?.[0] ? time[0].format('YYYY-MM-DD') : undefined;
      const endDate = time?.[1] ? time[1].format('YYYY-MM-DD') : undefined;

      const payload = {
        Name: name,
        Description: description,
        ManagerId: managerIdFromToken,
        StartDate: startDate,
        EndDate: endDate,
      };

      await api.post('/projects', payload);
      message.success('Tạo dự án thành công!');
      setIsModalVisible(false);
      fetchProjects();
      form.resetFields();
    } catch (e) {
      message.error('Tạo dự án thất bại!');
    }
  };

  const columns = [
    {
      title: 'Tên dự án',
      dataIndex: 'name',
      key: 'name',
      width: '35%',
      render: (text, record) => (
        <a
          onClick={() => {
            navigate(`/manager/projects/${record.projectId}/tasks`);
          }}
        >
          {text}
        </a>
      ),
    },
    {
      title: 'Quản lý',
      dataIndex: 'managerName',
      key: 'managerName',
      width: '25%',
      render: (v) => v || '—',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: '15%',
      render: (status) => {
        const color = status === 'Done' ? 'green' : status === 'Doing' ? 'blue' : 'gold';
        return <Tag color={color}>{status || 'N/A'}</Tag>;
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
            type="primary"
            onClick={() => navigate(`/manager/projects/${record.projectId}/tasks`)}
          >
            Chi tiết
          </Button>

          <Popconfirm
            title="Bạn có chắc chắn muốn xóa project này không?"
            okText="Có"
            cancelText="Không"
            onConfirm={async () => {
              try {
                await api.delete(`/projects/${record.projectId}`);
                message.success('Xóa project thành công!');
                fetchProjects();
              } catch (e) {
                message.error('Xóa project thất bại!');
              }
            }}
          >
            <Button size="small" danger>
              Xóa
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card
      title="Danh sách Dự án"
      extra={
        <Space>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreateProject}>
            Tạo Dự án mới
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
        dataSource={Array.isArray(projects) ? projects : []}
        rowKey="projectId"
        loading={loading}
        pagination={false}
        bordered
      />

      <Modal
        title="Tạo Dự án mới"
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        footer={null}
        destroyOnClose
      >
        <Form layout="vertical" form={form} onFinish={handleCreateProject}>
          <Form.Item name="name" label="Tên dự án" rules={[{ required: true, message: 'Vui lòng nhập tên dự án!' }]}>
            <Input placeholder="VD: Dự án MIS" />
          </Form.Item>

          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={3} placeholder="Mô tả dự án..." />
          </Form.Item>

          <Form.Item
            name="time"
            label="Ngày bắt đầu - Ngày kết thúc"
            rules={[{ required: true, message: 'Vui lòng chọn thời gian dự án!' }]}
          >
            <RangePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              Lưu dự án
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
}

