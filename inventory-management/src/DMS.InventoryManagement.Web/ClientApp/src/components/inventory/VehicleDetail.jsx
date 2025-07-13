import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { 
  Spin,
  Alert,
  Card, 
  Descriptions, 
  Button, 
  Tabs, 
  Table, 
  Tag, 
  Carousel, 
  Image, 
  Modal,
  Form,
  Input,
  InputNumber, 
  DatePicker,
  Select,
  Divider,
  Typography,
  Statistic,
  Row,
  Col
} from 'antd';
import {
  CarOutlined,
  DollarOutlined,
  CalendarOutlined,
  PictureOutlined,
  FilePdfOutlined,
  EditOutlined,
  DeleteOutlined,
  UploadOutlined,
  TagsOutlined,
  SettingOutlined
} from '@ant-design/icons';
import moment from 'moment';
import './VehicleDetail.css';
import { 
  getVehicleById, 
  updateVehicle, 
  uploadVehicleImage,
  uploadVehicleDocument,
  updateVehiclePricing,
  addVehicleFeature,
  deleteVehicleFeature
} from '../../services/inventoryService';

const { TabPane } = Tabs;
const { Title, Text } = Typography;
const { Option } = Select;

const VehicleDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [vehicle, setVehicle] = useState(null);
  
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [imageModalVisible, setImageModalVisible] = useState(false);
  const [documentModalVisible, setDocumentModalVisible] = useState(false);
  const [priceModalVisible, setPriceModalVisible] = useState(false);
  const [featureModalVisible, setFeatureModalVisible] = useState(false);
  
  const [form] = Form.useForm();
  const [priceForm] = Form.useForm();
  const [imageForm] = Form.useForm();
  const [documentForm] = Form.useForm();
  const [featureForm] = Form.useForm();

  useEffect(() => {
    const fetchVehicleData = async () => {
      try {
        setLoading(true);
        const data = await getVehicleById(id);
        setVehicle(data);
      } catch (err) {
        console.error('Error fetching vehicle data:', err);
        setError('Failed to load vehicle data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchVehicleData();
  }, [id]);

  const handleEditSubmit = async (values) => {
    try {
      await updateVehicle(id, values);
      setEditModalVisible(false);
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error updating vehicle:', err);
      // Show error notification
    }
  };

  const handlePriceSubmit = async (values) => {
    try {
      await updateVehiclePricing(id, values);
      setPriceModalVisible(false);
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error updating pricing:', err);
      // Show error notification
    }
  };

  const handleImageUpload = async (values) => {
    try {
      const formData = new FormData();
      formData.append('file', values.file.file);
      formData.append('isPrimary', values.isPrimary);
      formData.append('caption', values.caption);
      formData.append('imageType', values.imageType);
      
      await uploadVehicleImage(id, formData);
      setImageModalVisible(false);
      imageForm.resetFields();
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error uploading image:', err);
      // Show error notification
    }
  };

  const handleDocumentUpload = async (values) => {
    try {
      const formData = new FormData();
      formData.append('file', values.file.file);
      formData.append('title', values.title);
      formData.append('documentType', values.documentType);
      
      await uploadVehicleDocument(id, formData);
      setDocumentModalVisible(false);
      documentForm.resetFields();
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error uploading document:', err);
      // Show error notification
    }
  };

  const handleAddFeature = async (values) => {
    try {
      await addVehicleFeature(id, values);
      setFeatureModalVisible(false);
      featureForm.resetFields();
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error adding feature:', err);
      // Show error notification
    }
  };

  const handleDeleteFeature = async (featureId) => {
    try {
      await deleteVehicleFeature(id, featureId);
      
      // Refresh vehicle data
      const updatedData = await getVehicleById(id);
      setVehicle(updatedData);
    } catch (err) {
      console.error('Error deleting feature:', err);
      // Show error notification
    }
  };

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading vehicle details...</p>
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

  if (!vehicle) {
    return (
      <Alert
        message="Vehicle Not Found"
        description="The requested vehicle could not be found."
        type="warning"
        showIcon
      />
    );
  }

  const featureColumns = [
    {
      title: 'Feature',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, record) => (
        <Button 
          type="text" 
          danger 
          icon={<DeleteOutlined />}
          onClick={() => handleDeleteFeature(record.id)}
        />
      ),
    },
  ];

  const reconColumns = [
    {
      title: 'Date',
      dataIndex: 'workDate',
      key: 'workDate',
      render: date => moment(date).format('MM/DD/YYYY')
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Vendor',
      dataIndex: 'vendor',
      key: 'vendor',
    },
    {
      title: 'Cost',
      dataIndex: 'cost',
      key: 'cost',
      render: cost => `$${cost.toLocaleString()}`
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: status => {
        const colors = {
          Scheduled: 'blue',
          InProgress: 'orange',
          Completed: 'green',
          Cancelled: 'red'
        };
        return <Tag color={colors[status]}>{status}</Tag>;
      }
    },
  ];

  const documentColumns = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
    },
    {
      title: 'Type',
      dataIndex: 'documentType',
      key: 'documentType',
    },
    {
      title: 'Uploaded',
      dataIndex: 'uploadDate',
      key: 'uploadDate',
      render: date => moment(date).format('MM/DD/YYYY')
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, record) => (
        <>
          <Button type="link" icon={<FilePdfOutlined />} href={record.filePath} target="_blank">View</Button>
          <Button type="text" danger icon={<DeleteOutlined />} />
        </>
      ),
    },
  ];

  const priceHistory = vehicle.pricingDetails?.priceHistory.map(ph => ({
    ...ph,
    key: ph.id
  })) || [];

  const priceColumns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: date => moment(date).format('MM/DD/YYYY')
    },
    {
      title: 'Price',
      dataIndex: 'price',
      key: 'price',
      render: price => `$${price.toLocaleString()}`
    },
    {
      title: 'Reason',
      dataIndex: 'reason',
      key: 'reason',
    },
    {
      title: 'Changed By',
      dataIndex: 'userId',
      key: 'userId',
    }
  ];

  const vehicleConditionTag = () => {
    switch(vehicle.vehicleType) {
      case 'New':
        return <Tag color="blue">New</Tag>;
      case 'CertifiedPreOwned':
        return <Tag color="purple">CPO</Tag>;
      default:
        return <Tag color="green">Used</Tag>;
    }
  };

  const vehicleStatusTag = () => {
    const colors = {
      InTransit: 'orange',
      Receiving: 'blue',
      InStock: 'green',
      Reconditioning: 'cyan',
      FrontLine: 'green',
      OnHold: 'purple',
      Sold: 'red',
      Delivered: 'red',
      Transferred: 'geekblue'
    };
    
    return <Tag color={colors[vehicle.status]}>{vehicle.status}</Tag>;
  };
  
  const daysInInventory = () => {
    const acquisitionDate = moment(vehicle.acquisitionDate);
    const today = moment();
    return today.diff(acquisitionDate, 'days');
  };

  const getAgingColor = () => {
    const days = vehicle.agingInfo?.daysInInventory || daysInInventory();
    if (days > 60) return "#ff4d4f";
    if (days > 30) return "#faad14";
    return "#52c41a";
  };

  return (
    <div className="vehicle-detail">
      <div className="vehicle-detail-header">
        <div className="vehicle-title">
          <Title level={2}>
            {vehicle.year} {vehicle.make} {vehicle.model} {vehicle.trim}
            &nbsp;
            {vehicleConditionTag()}
            {vehicleStatusTag()}
          </Title>
          <Text type="secondary">Stock# {vehicle.stockNumber} | VIN: {vehicle.vin}</Text>
        </div>
        <div className="vehicle-actions">
          <Button 
            type="primary" 
            icon={<EditOutlined />} 
            onClick={() => {
              form.setFieldsValue({
                stockNumber: vehicle.stockNumber,
                vin: vehicle.vin,
                make: vehicle.make,
                model: vehicle.model,
                year: vehicle.year,
                trim: vehicle.trim,
                exteriorColor: vehicle.exteriorColor,
                interiorColor: vehicle.interiorColor,
                mileage: vehicle.mileage,
                vehicleType: vehicle.vehicleType,
                status: vehicle.status,
                lotLocation: vehicle.lotLocation
              });
              setEditModalVisible(true);
            }}
          >
            Edit Vehicle
          </Button>
          <Button 
            icon={<DollarOutlined />}
            onClick={() => {
              priceForm.setFieldsValue({
                msrp: vehicle.pricingDetails?.msrp || vehicle.msrp,
                internetPrice: vehicle.pricingDetails?.internetPrice || vehicle.listPrice,
                stickingPrice: vehicle.pricingDetails?.stickingPrice || vehicle.listPrice,
                floorPrice: vehicle.pricingDetails?.floorPrice || (vehicle.listPrice * 0.95)
              });
              setPriceModalVisible(true);
            }}
          >
            Update Price
          </Button>
        </div>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={16}>
          <Card className="vehicle-images-card">
            {vehicle.images && vehicle.images.length > 0 ? (
              <Carousel autoplay className="vehicle-image-carousel">
                {vehicle.images.map(image => (
                  <div key={image.id}>
                    <div className="carousel-image-container">
                      <Image 
                        src={image.filePath} 
                        alt={image.caption || `${vehicle.year} ${vehicle.make} ${vehicle.model}`} 
                      />
                      {image.caption && <div className="image-caption">{image.caption}</div>}
                    </div>
                  </div>
                ))}
              </Carousel>
            ) : (
              <div className="no-images">
                <PictureOutlined style={{ fontSize: '64px', opacity: 0.5 }} />
                <p>No images available</p>
              </div>
            )}
            <div className="image-actions">
              <Button 
                type="primary" 
                icon={<UploadOutlined />}
                onClick={() => setImageModalVisible(true)}
              >
                Add Images
              </Button>
            </div>
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card className="vehicle-pricing-card">
            <div className="price-section">
              <Statistic 
                title="Internet Price" 
                value={vehicle.pricingDetails?.internetPrice || vehicle.listPrice} 
                precision={0}
                formatter={value => `$${value.toLocaleString()}`}
              />
            </div>
            
            <Descriptions column={1}>
              <Descriptions.Item label="MSRP">
                ${(vehicle.pricingDetails?.msrp || vehicle.msrp).toLocaleString()}
              </Descriptions.Item>
              {vehicle.pricingDetails?.specialPrice && (
                <Descriptions.Item label="Special Price">
                  <Tag color="red">${vehicle.pricingDetails.specialPrice.toLocaleString()}</Tag>
                  <br />
                  <small>
                    Valid {moment(vehicle.pricingDetails.specialStartDate).format('MM/DD/YYYY')} - {moment(vehicle.pricingDetails.specialEndDate).format('MM/DD/YYYY')}
                  </small>
                </Descriptions.Item>
              )}
            </Descriptions>
            
            <Divider />
            
            <div className="aging-section">
              <Statistic 
                title="Days in Inventory" 
                value={vehicle.agingInfo?.daysInInventory || daysInInventory()}
                suffix="days"
                valueStyle={{ color: getAgingColor() }}
              />
              {vehicle.agingInfo?.agingAlertLevel === 'Critical' && (
                <Alert message={vehicle.agingInfo.recommendedAction} type="error" showIcon />
              )}
              {vehicle.agingInfo?.agingAlertLevel === 'Warning' && (
                <Alert message={vehicle.agingInfo.recommendedAction} type="warning" showIcon />
              )}
            </div>
          </Card>
          
          <Card className="vehicle-cost-card">
            <Descriptions title="Cost Information" column={1} bordered>
              <Descriptions.Item label="Acquisition Cost">
                ${(vehicle.costDetails?.acquisitionCost || vehicle.acquisitionCost || 0).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Reconditioning">
                ${(vehicle.costDetails?.reconditioningCost || 0).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Transport">
                ${(vehicle.costDetails?.transportCost || 0).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Certification">
                ${(vehicle.costDetails?.certificationCost || 0).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Additional">
                ${(vehicle.costDetails?.additionalCosts?.reduce((sum, cost) => sum + cost.amount, 0) || 0).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Total Cost">
                <Text strong>${(vehicle.costDetails?.totalCost || vehicle.acquisitionCost || 0).toLocaleString()}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Target Profit">
                ${(vehicle.costDetails?.targetGrossProfit || 0).toLocaleString()}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>
      </Row>

      <Card className="vehicle-tabs-card">
        <Tabs defaultActiveKey="details">
          <TabPane tab="Details" key="details">
            <Descriptions title="Vehicle Specifications" bordered column={{ xxl: 4, xl: 3, lg: 3, md: 3, sm: 2, xs: 1 }}>
              <Descriptions.Item label="Stock Number">{vehicle.stockNumber}</Descriptions.Item>
              <Descriptions.Item label="VIN">{vehicle.vin}</Descriptions.Item>
              <Descriptions.Item label="Year">{vehicle.year}</Descriptions.Item>
              <Descriptions.Item label="Make">{vehicle.make}</Descriptions.Item>
              <Descriptions.Item label="Model">{vehicle.model}</Descriptions.Item>
              <Descriptions.Item label="Trim">{vehicle.trim}</Descriptions.Item>
              <Descriptions.Item label="Exterior Color">{vehicle.exteriorColor}</Descriptions.Item>
              <Descriptions.Item label="Interior Color">{vehicle.interiorColor}</Descriptions.Item>
              <Descriptions.Item label="Mileage">{vehicle.mileage.toLocaleString()} miles</Descriptions.Item>
              <Descriptions.Item label="Type">{vehicle.vehicleType}</Descriptions.Item>
              <Descriptions.Item label="Status">{vehicle.status}</Descriptions.Item>
              <Descriptions.Item label="Location">{vehicle.location?.name || vehicle.lotLocation}</Descriptions.Item>
            </Descriptions>
            
            <Divider />
            
            <div className="vehicle-features">
              <div className="features-header">
                <Title level={4}>Features</Title>
                <Button 
                  type="primary" 
                  icon={<TagsOutlined />} 
                  onClick={() => setFeatureModalVisible(true)}
                >
                  Add Feature
                </Button>
              </div>
              <Table 
                dataSource={vehicle.features} 
                columns={featureColumns}
                rowKey="id"
                pagination={false}
                size="small"
              />
            </div>
          </TabPane>
          
          <TabPane tab="Images" key="images">
            <div className="image-gallery">
              {vehicle.images && vehicle.images.length > 0 ? (
                <div className="image-grid">
                  {vehicle.images.map(image => (
                    <div key={image.id} className="image-item">
                      <Image 
                        src={image.filePath} 
                        alt={image.caption || `${vehicle.year} ${vehicle.make} ${vehicle.model}`}
                      />
                      <div className="image-info">
                        <div>
                          {image.caption || 'No caption'}
                          {image.isPrimary && <Tag color="blue">Primary</Tag>}
                          <Tag color="purple">{image.imageType}</Tag>
                        </div>
                        <div className="image-actions">
                          <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                          <Button type="text" icon={<EditOutlined />} size="small" />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="no-data">
                  <PictureOutlined style={{ fontSize: '64px', opacity: 0.5 }} />
                  <p>No images available</p>
                  <Button 
                    type="primary" 
                    icon={<UploadOutlined />}
                    onClick={() => setImageModalVisible(true)}
                  >
                    Add Images
                  </Button>
                </div>
              )}
            </div>
          </TabPane>
          
          <TabPane tab="Reconditioning" key="reconditioning">
            <Table 
              dataSource={vehicle.reconditioningRecords} 
              columns={reconColumns}
              rowKey="id"
              pagination={false}
            />
            
            <div className="table-actions">
              <Button type="primary" icon={<SettingOutlined />}>Add Reconditioning Work</Button>
            </div>
          </TabPane>
          
          <TabPane tab="Documents" key="documents">
            <Table 
              dataSource={vehicle.documents} 
              columns={documentColumns}
              rowKey="id"
              pagination={false}
            />
            
            <div className="table-actions">
              <Button 
                type="primary" 
                icon={<UploadOutlined />}
                onClick={() => setDocumentModalVisible(true)}
              >
                Upload Document
              </Button>
            </div>
          </TabPane>
          
          <TabPane tab="Pricing History" key="pricing">
            <Table 
              dataSource={priceHistory} 
              columns={priceColumns}
              rowKey="id"
              pagination={false}
            />
          </TabPane>
          
          <TabPane tab="History" key="history">
            <Timeline>
              {/* History timeline would go here */}
              <Timeline.Item>Vehicle acquired on {moment(vehicle.acquisitionDate).format('MM/DD/YYYY')}</Timeline.Item>
              <Timeline.Item>Added to inventory on {moment(vehicle.createdAt).format('MM/DD/YYYY')}</Timeline.Item>
            </Timeline>
          </TabPane>
        </Tabs>
      </Card>

      {/* Edit Vehicle Modal */}
      <Modal
        title="Edit Vehicle"
        visible={editModalVisible}
        footer={null}
        onCancel={() => setEditModalVisible(false)}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleEditSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="stockNumber"
                label="Stock Number"
                rules={[{ required: true, message: 'Please enter the stock number' }]}
              >
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="vin"
                label="VIN"
                rules={[{ required: true, message: 'Please enter the VIN' }]}
              >
                <Input />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item
                name="year"
                label="Year"
                rules={[{ required: true, message: 'Please enter the year' }]}
              >
                <InputNumber min={1900} max={2100} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="make"
                label="Make"
                rules={[{ required: true, message: 'Please enter the make' }]}
              >
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="model"
                label="Model"
                rules={[{ required: true, message: 'Please enter the model' }]}
              >
                <Input />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="trim" label="Trim">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="exteriorColor" label="Exterior Color">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="interiorColor" label="Interior Color">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item
                name="mileage"
                label="Mileage"
                rules={[{ required: true, message: 'Please enter the mileage' }]}
              >
                <InputNumber min={0} style={{ width: '100%' }} formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="vehicleType"
                label="Type"
                rules={[{ required: true, message: 'Please select the vehicle type' }]}
              >
                <Select>
                  <Option value="New">New</Option>
                  <Option value="Used">Used</Option>
                  <Option value="CertifiedPreOwned">Certified Pre-Owned</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="status"
                label="Status"
                rules={[{ required: true, message: 'Please select the status' }]}
              >
                <Select>
                  <Option value="InTransit">In Transit</Option>
                  <Option value="Receiving">Receiving</Option>
                  <Option value="InStock">In Stock</Option>
                  <Option value="Reconditioning">Reconditioning</Option>
                  <Option value="FrontLine">Front Line</Option>
                  <Option value="OnHold">On Hold</Option>
                  <Option value="Sold">Sold</Option>
                  <Option value="Delivered">Delivered</Option>
                  <Option value="Transferred">Transferred</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item name="lotLocation" label="Lot Location">
            <Input />
          </Form.Item>
          
          <Form.Item>
            <Button type="primary" htmlType="submit">Save Changes</Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setEditModalVisible(false)}>Cancel</Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* Update Pricing Modal */}
      <Modal
        title="Update Pricing"
        visible={priceModalVisible}
        footer={null}
        onCancel={() => setPriceModalVisible(false)}
      >
        <Form
          form={priceForm}
          layout="vertical"
          onFinish={handlePriceSubmit}
        >
          <Form.Item
            name="msrp"
            label="MSRP"
            rules={[{ required: true, message: 'Please enter the MSRP' }]}
          >
            <InputNumber 
              min={0} 
              style={{ width: '100%' }} 
              formatter={value => `$${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={value => value.replace(/\$\s?|(,*)/g, '')}
            />
          </Form.Item>
          
          <Form.Item
            name="internetPrice"
            label="Internet Price"
            rules={[{ required: true, message: 'Please enter the internet price' }]}
          >
            <InputNumber 
              min={0} 
              style={{ width: '100%' }} 
              formatter={value => `$${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={value => value.replace(/\$\s?|(,*)/g, '')}
            />
          </Form.Item>
          
          <Form.Item
            name="stickingPrice"
            label="Sticking Price"
            rules={[{ required: true, message: 'Please enter the sticking price' }]}
          >
            <InputNumber 
              min={0} 
              style={{ width: '100%' }} 
              formatter={value => `$${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={value => value.replace(/\$\s?|(,*)/g, '')}
            />
          </Form.Item>
          
          <Form.Item
            name="floorPrice"
            label="Floor Price"
            rules={[{ required: true, message: 'Please enter the floor price' }]}
          >
            <InputNumber 
              min={0} 
              style={{ width: '100%' }} 
              formatter={value => `$${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={value => value.replace(/\$\s?|(,*)/g, '')}
            />
          </Form.Item>
          
          <Divider />
          
          <Form.Item name="specialPrice" label="Special Price">
            <InputNumber 
              min={0} 
              style={{ width: '100%' }} 
              formatter={value => value ? `$${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',') : ''}
              parser={value => value ? value.replace(/\$\s?|(,*)/g, '') : ''}
            />
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="specialStartDate" label="Special Start Date">
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="specialEndDate" label="Special End Date">
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item name="reason" label="Reason for Price Change">
            <Input.TextArea rows={3} />
          </Form.Item>
          
          <Form.Item>
            <Button type="primary" htmlType="submit">Update Pricing</Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setPriceModalVisible(false)}>Cancel</Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* Upload Image Modal */}
      <Modal
        title="Upload Vehicle Images"
        visible={imageModalVisible}
        footer={null}
        onCancel={() => setImageModalVisible(false)}
      >
        <Form
          form={imageForm}
          layout="vertical"
          onFinish={handleImageUpload}
        >
          <Form.Item
            name="file"
            label="Image File"
            rules={[{ required: true, message: 'Please select an image to upload' }]}
          >
            <Upload.Dragger 
              name="file" 
              beforeUpload={() => false} 
              maxCount={1} 
              accept="image/*"
              listType="picture"
            >
              <p className="ant-upload-drag-icon">
                <PictureOutlined />
              </p>
              <p className="ant-upload-text">Click or drag image to this area to upload</p>
              <p className="ant-upload-hint">
                Support for a single image upload. Please use high quality images.
              </p>
            </Upload.Dragger>
          </Form.Item>
          
          <Form.Item name="caption" label="Caption">
            <Input placeholder="Optional caption for the image" />
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="imageType"
                label="Image Type"
                rules={[{ required: true, message: 'Please select the image type' }]}
                initialValue="Exterior"
              >
                <Select>
                  <Option value="Exterior">Exterior</Option>
                  <Option value="Interior">Interior</Option>
                  <Option value="Damage">Damage</Option>
                  <Option value="Feature">Feature</Option>
                  <Option value="Other">Other</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="isPrimary"
                valuePropName="checked"
                initialValue={false}
              >
                <Checkbox style={{ marginTop: 37 }}>Set as primary image</Checkbox>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item>
            <Button type="primary" htmlType="submit">Upload Image</Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setImageModalVisible(false)}>Cancel</Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* Upload Document Modal */}
      <Modal
        title="Upload Vehicle Document"
        visible={documentModalVisible}
        footer={null}
        onCancel={() => setDocumentModalVisible(false)}
      >
        <Form
          form={documentForm}
          layout="vertical"
          onFinish={handleDocumentUpload}
        >
          <Form.Item
            name="file"
            label="Document File"
            rules={[{ required: true, message: 'Please select a document to upload' }]}
          >
            <Upload.Dragger 
              name="file" 
              beforeUpload={() => false} 
              maxCount={1} 
              accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
            >
              <p className="ant-upload-drag-icon">
                <FilePdfOutlined />
              </p>
              <p className="ant-upload-text">Click or drag document to this area to upload</p>
              <p className="ant-upload-hint">
                Support for PDF, Word documents and images.
              </p>
            </Upload.Dragger>
          </Form.Item>
          
          <Form.Item
            name="title"
            label="Document Title"
            rules={[{ required: true, message: 'Please enter the document title' }]}
          >
            <Input placeholder="Document title" />
          </Form.Item>
          
          <Form.Item
            name="documentType"
            label="Document Type"
            rules={[{ required: true, message: 'Please select the document type' }]}
          >
            <Select>
              <Option value="Title">Title</Option>
              <Option value="Registration">Registration</Option>
              <Option value="ServiceRecord">Service Record</Option>
              <Option value="WarrantyInformation">Warranty Information</Option>
              <Option value="PurchaseAgreement">Purchase Agreement</Option>
              <Option value="InspectionReport">Inspection Report</Option>
              <Option value="Odometer">Odometer</Option>
              <Option value="EmissionTest">Emission Test</Option>
              <Option value="CarfaxReport">Carfax Report</Option>
              <Option value="Other">Other</Option>
            </Select>
          </Form.Item>
          
          <Form.Item>
            <Button type="primary" htmlType="submit">Upload Document</Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setDocumentModalVisible(false)}>Cancel</Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* Add Feature Modal */}
      <Modal
        title="Add Vehicle Feature"
        visible={featureModalVisible}
        footer={null}
        onCancel={() => setFeatureModalVisible(false)}
      >
        <Form
          form={featureForm}
          layout="vertical"
          onFinish={handleAddFeature}
        >
          <Form.Item
            name="name"
            label="Feature Name"
            rules={[{ required: true, message: 'Please enter the feature name' }]}
          >
            <Input placeholder="Feature name" />
          </Form.Item>
          
          <Form.Item name="category" label="Category">
            <Select allowClear>
              <Option value="Safety">Safety</Option>
              <Option value="Performance">Performance</Option>
              <Option value="Comfort">Comfort</Option>
              <Option value="Technology">Technology</Option>
              <Option value="Exterior">Exterior</Option>
              <Option value="Interior">Interior</Option>
              <Option value="Entertainment">Entertainment</Option>
              <Option value="Other">Other</Option>
            </Select>
          </Form.Item>
          
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Optional description" />
          </Form.Item>
          
          <Form.Item>
            <Button type="primary" htmlType="submit">Add Feature</Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setFeatureModalVisible(false)}>Cancel</Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default VehicleDetail;
