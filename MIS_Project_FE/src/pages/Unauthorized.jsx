import React from 'react';
import { Card, Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';

export default function Unauthorized() {
  const navigate = useNavigate();

  return (
    <div style={{ padding: 24, minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <Card style={{ width: 520, maxWidth: '100%' }}>
        <Result
          status="403"
          title="Không có quyền truy cập"
          subTitle="Tài khoản của bạn không được phép vào trang này."
          extra={[
            <Button key="back" type="primary" onClick={() => navigate(-1)}>
              Quay lại
            </Button>,
            <Button key="login" onClick={() => navigate('/login')}>
              Đăng nhập lại
            </Button>
          ]}
        />
      </Card>
    </div>
  );
}

