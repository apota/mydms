import React, { useState, useEffect } from 'react';
import { 
  Card, Row, Col, Statistic, Form, Input, InputNumber, Button, Tabs, 
  Alert, Spin, Divider, Table, Tag, Tooltip, Typography, Modal
} from 'antd';
import { 
  DollarOutlined, LineChartOutlined, HistoryOutlined, 
  InfoCircleOutlined, CheckOutlined, ArrowUpOutlined, ArrowDownOutlined
} from '@ant-design/icons';
import { useParams } from 'react-router-dom';
import { 
  getVehicleById, 
  getVehiclePricing, 
  updateVehiclePricing, 
  getPricingHistory,
  getMarketAnalysis
} from '../../services/inventoryService';

const { TabPane } = Tabs;
const { Title, Text } = Typography;

const VehiclePricing = () => {
  const { id } = useParams();
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [vehicle, setVehicle] = useState(null);
  const [pricing, setPricing] = useState(null);
  const [history, setPricingHistory] = useState([]);
  const [marketData, setMarketData] = useState(null);
  
  const [confirmModalVisible, setConfirmModalVisible] = useState(false);
  const [newPriceData, setNewPriceData] = useState(null);
  
  const [form] = Form.useForm();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Fetch vehicle and pricing data in parallel
        const [vehicleData, pricingData, historyData, marketAnalysisData] = await Promise.all([
          getVehicleById(id),
          getVehiclePricing(id),
          getPricingHistory(id),
          getMarketAnalysis(id)
        ]);
        
        setVehicle(vehicleData);
        setPricing(pricingData);
        setPricingHistory(historyData);
        setMarketData(marketAnalysisData);
        
        // Initialize form with current pricing data
        form.setFieldsValue({
          listPrice: pricingData.listPrice,
          internetPrice: pricingData.internetPrice,
          msrp: pricingData.msrp,
          invoicePrice: pricingData.invoicePrice,
          minimumSellingPrice: pricingData.minimumSellingPrice,
          notes: pricingData.notes
        });
      } catch (err) {
        console.error('Error fetching pricing data:', err);
        setError('Failed to load pricing data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [id, form]);

  const handlePriceSave = (values) => {
    setNewPriceData(values);
    setConfirmModalVisible(true);
  };
    const confirmPriceUpdate = async () => {
    try {
      setLoading(true);
      
      // Add reason for the price change
      const pricingUpdate = {
        ...newPriceData,
        changeReason: 'Manual adjustment by user'  // Could be from a dropdown in a real implementation
      };
      
      await updateVehiclePricing(id, pricingUpdate);
      
      // Refresh pricing data and history
      const [refreshedPricing, updatedHistory] = await Promise.all([
        getVehiclePricing(id),
        getPricingHistory(id)
      ]);
      
      setPricing(refreshedPricing);
      setPricingHistory(updatedHistory);
      
      setConfirmModalVisible(false);
    } catch (err) {
      console.error('Error updating pricing:', err);
      setError('Failed to update pricing. Please try again later.');
    } finally {
      setLoading(false);
    }
  };
  
  // Calculate variance between our price and market average
  const calculateVariance = () => {
    if (!pricing || !marketData || !marketData.averagePrice) return 0;
    
    const variance = pricing.listPrice - marketData.averagePrice;
    return {
      amount: variance,
      percentage: ((variance / marketData.averagePrice) * 100).toFixed(1)
    };
  };
  
  // Prepare pricing history for display in table
  const pricingHistoryColumns = [
    {
      title: 'Date',
      dataIndex: 'changeDate',
      key: 'changeDate',
      render: (date) => new Date(date).toLocaleDateString()
    },
    {
      title: 'Previous Price',
      dataIndex: 'previousPrice',
      key: 'previousPrice',
      render: (price) => `$${price?.toLocaleString() || 0}`
    },
    {
      title: 'New Price',
      dataIndex: 'newPrice',
      key: 'newPrice',
      render: (price, record) => {
        const change = record.newPrice - record.previousPrice;
        const color = change < 0 ? 'green' : change > 0 ? 'red' : 'default';
        const icon = change < 0 ? <ArrowDownOutlined /> : change > 0 ? <ArrowUpOutlined /> : null;
        
        return (
          <Text type={color === 'default' ? null : color}>
            ${price.toLocaleString()} {icon}
          </Text>
        );
      }
    },
    {
      title: 'Change',
      dataIndex: 'newPrice',
      key: 'change',
      render: (_, record) => {
        const change = record.newPrice - record.previousPrice;
        const color = change < 0 ? 'green' : change > 0 ? 'red' : '';
        const prefix = change >= 0 ? '+' : '';
        
        return (
          <Text type={color}>
            {prefix}${change.toLocaleString()}
            {' '}
            ({prefix}{((change / record.previousPrice) * 100).toFixed(1)}%)
          </Text>
        );
      }
    },
    {
      title: 'Reason',
      dataIndex: 'reason',
      key: 'reason'
    },
    {
      title: 'Changed By',
      dataIndex: 'changedBy',
      key: 'changedBy'
    }
  ];

  const marketComparisonColumns = [
    {
      title: 'Source',
      dataIndex: 'source',
      key: 'source'
    },
    {
      title: 'Year',
      dataIndex: 'year',
      key: 'year'
    },
    {
      title: 'Make/Model',
      dataIndex: 'makeModel',
      key: 'makeModel'
    },
    {
      title: 'Trim',
      dataIndex: 'trim',
      key: 'trim'
    },
    {
      title: 'Mileage',
      dataIndex: 'mileage',
      key: 'mileage',
      render: (mileage) => mileage.toLocaleString()
    },
    {
      title: 'Price',
      dataIndex: 'price',
      key: 'price',
      render: (price) => `$${price.toLocaleString()}`
    },
    {
      title: 'Days Listed',
      dataIndex: 'daysListed',
      key: 'daysListed'
    },
    {
      title: 'Distance',
      dataIndex: 'distance',
      key: 'distance',
      render: (distance) => `${distance} miles`
    }
  ];

  if (loading && !vehicle) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading pricing data...</p>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error"
        description={error}
        type="error"
        showIcon
      />
    );
  }

  const variance = calculateVariance();

  return (
    <div className="vehicle-pricing">
      <Row gutter={[16, 16]} className="pricing-header">
        <Col span={24}>
          <Title level={2}>
            <DollarOutlined /> Pricing Manager
          </Title>
          {vehicle && (
            <Title level={4}>
              {vehicle.year} {vehicle.make} {vehicle.model} - Stock #{vehicle.stockNumber}
            </Title>
          )}
        </Col>
      </Row>
      
      <Tabs defaultActiveKey="1">
        <TabPane 
          tab={
            <span>
              <DollarOutlined />
              Pricing Information
            </span>
          } 
          key="1"
        >
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={16}>
              <Card title="Current Pricing">
                {loading ? (
                  <div style={{ textAlign: 'center', padding: '20px' }}>
                    <Spin />
                  </div>
                ) : (
                  <Form
                    form={form}
                    layout="vertical"
                    onFinish={handlePriceSave}
                  >
                    <Row gutter={16}>
                      <Col xs={24} sm={12}>
                        <Form.Item 
                          name="listPrice" 
                          label="List Price ($)" 
                          rules={[{ required: true, message: 'List price is required' }]}
                        >
                          <InputNumber
                            style={{ width: '100%' }}
                            formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                            parser={value => value.replace(/\$\s?|(,*)/g, '')}
                          />
                        </Form.Item>
                      </Col>
                      <Col xs={24} sm={12}>
                        <Form.Item 
                          name="internetPrice" 
                          label="Internet Price ($)" 
                          rules={[{ required: true, message: 'Internet price is required' }]}
                        >
                          <InputNumber
                            style={{ width: '100%' }}
                            formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                            parser={value => value.replace(/\$\s?|(,*)/g, '')}
                          />
                        </Form.Item>
                      </Col>
                    </Row>
                    
                    <Row gutter={16}>
                      <Col xs={24} sm={12}>
                        <Form.Item 
                          name="msrp" 
                          label="MSRP ($)"
                        >
                          <InputNumber
                            style={{ width: '100%' }}
                            formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                            parser={value => value.replace(/\$\s?|(,*)/g, '')}
                          />
                        </Form.Item>
                      </Col>
                      <Col xs={24} sm={12}>
                        <Form.Item 
                          name="invoicePrice" 
                          label="Invoice Price ($)"
                        >
                          <InputNumber
                            style={{ width: '100%' }}
                            formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                            parser={value => value.replace(/\$\s?|(,*)/g, '')}
                          />
                        </Form.Item>
                      </Col>
                    </Row>
                    
                    <Form.Item 
                      name="minimumSellingPrice" 
                      label="Minimum Selling Price ($)"
                      rules={[{ required: true, message: 'Minimum selling price is required' }]}
                    >
                      <InputNumber
                        style={{ width: '100%' }}
                        formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                        parser={value => value.replace(/\$\s?|(,*)/g, '')}
                      />
                    </Form.Item>
                    
                    <Form.Item 
                      name="notes" 
                      label="Pricing Notes"
                    >
                      <Input.TextArea 
                        rows={4}
                        placeholder="Add notes about pricing strategy or special considerations"
                      />
                    </Form.Item>
                    
                    <Form.Item>
                      <Button type="primary" htmlType="submit">
                        Update Pricing
                      </Button>
                    </Form.Item>
                  </Form>
                )}
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card title="Market Position">
                {marketData ? (
                  <>
                    <Statistic
                      title="Market Average Price"
                      value={marketData.averagePrice}
                      precision={0}
                      formatter={(value) => `$${value.toLocaleString()}`}
                    />
                    
                    <Divider />
                    
                    <Statistic
                      title="Your Price vs Market"
                      value={variance.percentage}
                      precision={1}
                      valueStyle={{ color: variance.amount > 0 ? '#cf1322' : '#3f8600' }}
                      prefix={variance.amount > 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                      suffix="%"
                    />
                    
                    <Text type={variance.amount > 0 ? 'danger' : 'success'}>
                      {variance.amount > 0 ? 'Above' : 'Below'} market by ${Math.abs(variance.amount).toLocaleString()}
                    </Text>
                    
                    <Divider />
                    
                    <div className="market-metrics">
                      <div className="metric">
                        <Text>Average Days to Sell:</Text>
                        <Text strong>{marketData.averageDaysToSell} days</Text>
                      </div>
                      
                      <div className="metric">
                        <Text>Similar Vehicles in Market:</Text>
                        <Text strong>{marketData.similarVehiclesCount}</Text>
                      </div>
                      
                      <div className="metric">
                        <Text>Market Demand:</Text>
                        <Tag color={marketData.demandLevel === 'High' ? 'green' : marketData.demandLevel === 'Medium' ? 'blue' : 'orange'}>
                          {marketData.demandLevel}
                        </Tag>
                      </div>
                    </div>
                    
                    <Divider />
                    
                    <div className="price-recommendation">
                      <Title level={4}>Price Recommendation</Title>
                      <Alert
                        message={`Recommended: $${marketData.recommendedPrice.toLocaleString()}`}
                        description={marketData.recommendationReason}
                        type="info"
                        showIcon
                        icon={<InfoCircleOutlined />}
                      />
                      <Button 
                        type="primary" 
                        style={{ marginTop: '16px' }}
                        onClick={() => {
                          form.setFieldsValue({ listPrice: marketData.recommendedPrice });
                        }}
                      >
                        Apply Recommendation
                      </Button>
                    </div>
                  </>
                ) : (
                  <div style={{ textAlign: 'center', padding: '20px' }}>
                    <Spin />
                    <p>Loading market data...</p>
                  </div>
                )}
              </Card>
            </Col>
          </Row>
        </TabPane>
        
        <TabPane 
          tab={
            <span>
              <HistoryOutlined />
              Pricing History
            </span>
          } 
          key="2"
        >
          <Card title="Price Change History">
            <Table 
              dataSource={history} 
              columns={pricingHistoryColumns}
              rowKey="id"
              pagination={{ pageSize: 10 }}
              loading={loading}
            />
          </Card>
        </TabPane>
        
        <TabPane 
          tab={
            <span>
              <LineChartOutlined />
              Market Comparison
            </span>
          } 
          key="3"
        >
          <Card title="Comparable Vehicles in Market">
            {marketData?.comparableVehicles ? (
              <Table 
                dataSource={marketData.comparableVehicles} 
                columns={marketComparisonColumns}
                rowKey="id"
                pagination={{ pageSize: 10 }}
                loading={loading}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '20px' }}>
                <Spin />
                <p>Loading market comparison data...</p>
              </div>
            )}
          </Card>
        </TabPane>
      </Tabs>
      
      <Modal
        title="Confirm Price Change"
        visible={confirmModalVisible}
        onOk={confirmPriceUpdate}
        onCancel={() => setConfirmModalVisible(false)}
        okText="Yes, Update Price"
        cancelText="Cancel"
        confirmLoading={loading}
      >
        <p>Are you sure you want to update the pricing for this vehicle?</p>
        
        {newPriceData && pricing && (
          <div>
            <p>
              <strong>List Price:</strong>{' '}
              <Text delete>${pricing.listPrice.toLocaleString()}</Text>{' → '}
              <Text strong>${newPriceData.listPrice.toLocaleString()}</Text>
            </p>
            
            <p>
              <strong>Internet Price:</strong>{' '}
              <Text delete>${pricing.internetPrice.toLocaleString()}</Text>{' → '}
              <Text strong>${newPriceData.internetPrice.toLocaleString()}</Text>
            </p>
            
            <p>This change will be logged in the pricing history.</p>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default VehiclePricing;
