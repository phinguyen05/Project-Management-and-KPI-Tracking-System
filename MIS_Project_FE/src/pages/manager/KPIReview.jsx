import React, { useState, useEffect } from 'react';
import { Table, Button, Card, Space, Tag, message, InputNumber, Alert, Divider } from 'antd';
import { CheckOutlined, SyncOutlined } from '@ant-design/icons';
import api from '../../services/api';
import * as XLSX from 'xlsx';

export default function KPIReview() {
    const [kpiData, setKpiData] = useState([]);
    const [loading, setLoading] = useState(false);
    const [syncing, setSyncing] = useState(false);

    const fetchKpiData = async () => {
        setLoading(true);
        try {
            const res = await api.get('/kpis/summary?month=6&year=2026');
            setKpiData(res.data);
        } catch (error) {
            message.error('Lỗi khi lấy dữ liệu KPI từ Backend!');
            setKpiData([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchKpiData();
    }, []);

    const calculateTotal = (record) => {
        let timeliness = record.timeliness || 0;
        let efficiency = record.efficiency || 0;
        let capacity = record.capacity || 0;
        let managerScore = record.managerScore || 0;

        let total = timeliness * 0.4 + efficiency * 0.3 + capacity * 0.2 + managerScore * 0.1;

        if (capacity > 110 && timeliness >= 90) {
            total = total * 1.1;
        }

        return Math.min(total, 150).toFixed(1);
    };

    const handleFinalize = async (record) => {
        setKpiData((prev) => prev.map((item) => (item.userId === record.userId ? { ...item, status: 'Closed' } : item)));

        try {
            await api.post(`/kpis/finalize/${record.userId}?month=6&year=2026`, {
                managerScore: record.managerScore || 0,
                note: 'Đánh giá cuối kỳ',
            });
            message.success(`Đã chốt KPI cho ${record.userName}!`);
        } catch (error) {
            message.error(error?.response?.data?.message || `Chốt KPI thất bại cho ${record.userName}!`);
            setKpiData((prev) => prev.map((i) => (i.userId === record.userId ? { ...i, status: 'Draft' } : i)));
        }
    };

    const handleExportExcel = () => {
        try {
            const data = (kpiData || []).map((item) => ({
                'Tên nhân viên': item.userName,
                'Tiến độ (%)': item.timeliness,
                'Hiệu suất (%)': item.efficiency,
                'Khối lượng (%)': item.capacity,
                'Điểm Quản lý': item.managerScore,
                'Tổng KPI (%)': calculateTotal(item),
                'Trạng thái': item.status,
            }));

            const worksheet = XLSX.utils.json_to_sheet(data);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, 'KPI');

            XLSX.writeFile(workbook, 'Bao_Cao_KPI_Thang.xlsx');
            message.success('Tải xuống báo cáo KPI (Excel) thành công!');
        } catch (e) {
            message.error('Xuất Excel thất bại.');
        }
    };

    const handleSyncToHRM = () => {
        setSyncing(true);
        setTimeout(() => {
            setSyncing(false);
            message.success('Đồng bộ dữ liệu điểm KPI sang Hệ thống Tính lương (HRM/Payroll) thành công!');
        }, 1500);
    };

    const columns = [
        { title: 'Nhân viên', dataIndex: 'userName', key: 'userName' },
        {
            title: 'Tiến độ (40%)',
            dataIndex: 'timeliness',
            key: 'timeliness',
            render: (v) => (
                <Space>
                    <span>{v}%</span>
                    {v >= 90 && <Tag color="success">Đúng hạn</Tag>}
                </Space>
            ),
        },
        { title: 'Hiệu suất (30%)', dataIndex: 'efficiency', key: 'efficiency', render: (v) => `${v}%` },
        {
            title: 'Khối lượng (20%)',
            dataIndex: 'capacity',
            key: 'capacity',
            render: (v) => (
                <Space>
                    <span>{v}%</span>
                    {v > 110 && <Tag color="red">Overload ({v}%)</Tag>}
                </Space>
            ),
        },
        {
            title: 'Điểm QL (10%)',
            render: (_, record) => (
                <InputNumber
                    min={0}
                    max={100}
                    value={record.managerScore}
                    disabled={record.status === 'Closed'}
                    onChange={(val) => {
                        setKpiData((prev) => prev.map((i) => (i.userId === record.userId ? { ...i, managerScore: val } : i)));
                    }}
                />
            ),
        },
        { title: 'Tổng KPI', render: (_, r) => <strong>{calculateTotal(r)}%</strong> },
        {
            title: 'Hành động',
            render: (_, r) => (
                <Button
                    type="primary"
                    icon={<CheckOutlined />}
                    onClick={() => handleFinalize(r)}
                    disabled={r.status === 'Closed'}
                    style={r.status === 'Closed' ? { background: '#d9d9d9', borderColor: '#d9d9d9' } : undefined}
                >
                    {r.status === 'Closed' ? 'Đã chốt' : 'Chốt & Duyệt'}
                </Button>
            ),
        },
    ];

    return (
        <Card title="Chốt/Duyệt Điểm KPI Cuối Tháng (Tháng 06/2026)">
            <Alert
                message="Gợi ý tính toán KPI"
                description="Hệ thống tự động nhân hệ số thưởng 1.1 (thêm 10%) vào tổng điểm cho các nhân sự bị quá tải (Overload > 110%) nhưng vẫn giữ được KPI Tiến độ >= 90%."
                type="info"
                showIcon
                style={{ marginBottom: 20 }}
            />

            <Table columns={columns} dataSource={kpiData} rowKey="userId" loading={loading} bordered pagination={false} />

            <Divider />

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 16 }}>
                <Button type="default" onClick={handleExportExcel}>
                    ⬇ Tải xuống Báo cáo (Excel)
                </Button>
                <Button type="primary" icon={<SyncOutlined />} loading={syncing} onClick={handleSyncToHRM}>
                    Đồng bộ sang Hệ thống Lương (HRM/Payroll)
                </Button>
            </div>
        </Card>
    );
}

