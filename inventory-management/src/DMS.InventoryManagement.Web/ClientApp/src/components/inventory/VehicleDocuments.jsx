import React, { useState, useEffect } from 'react';
import { 
  Table, Card, Button, Upload, Modal, Form, Input, Select, 
  Spin, Alert, Tooltip, Tag, Typography, Popconfirm, Divider
} from 'antd';
import { 
  FilePdfOutlined, FileImageOutlined, FileExcelOutlined, FileWordOutlined, 
  FileUnknownOutlined, UploadOutlined, DownloadOutlined, DeleteOutlined, EyeOutlined
} from '@ant-design/icons';
import { useParams } from 'react-router-dom';
import { getVehicleById, getVehicleDocuments, uploadVehicleDocument, deleteVehicleDocument } from '../../services/inventoryService';

const { Title, Text } = Typography;
const { Option } = Select;
const { Dragger } = Upload;

const VehicleDocuments = () => {
  const { id } = useParams();
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [vehicle, setVehicle] = useState(null);
  const [documents, setDocuments] = useState([]);
  const [uploadModalVisible, setUploadModalVisible] = useState(false);
  const [previewModalVisible, setPreviewModalVisible] = useState(false);
  const [previewDocument, setPreviewDocument] = useState(null);
  
  const [form] = Form.useForm();
  
  useEffect(() => {
    fetchData();
  }, [id]);
  
  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Fetch vehicle and documents data in parallel
      const [vehicleData, documentsData] = await Promise.all([
        getVehicleById(id),
        getVehicleDocuments(id)
      ]);
      
      setVehicle(vehicleData);
      setDocuments(documentsData);
    } catch (err) {
      console.error('Error fetching document data:', err);
      setError('Failed to load documents. Please try again later.');
    } finally {
      setLoading(false);
    }
  };
  
  const handleUpload = async (values) => {
    try {
      const { file, documentType, title, notes } = values;
      
      setLoading(true);
      
      await uploadVehicleDocument(id, file.file, {
        documentType,
        title,
        notes
      });
      
      form.resetFields();
      setUploadModalVisible(false);
      
      // Refresh document list
      const documentsData = await getVehicleDocuments(id);
      setDocuments(documentsData);
    } catch (err) {
      console.error('Error uploading document:', err);
      setError('Failed to upload document. Please try again later.');
    } finally {
      setLoading(false);
    }
  };
  
  const handleDelete = async (documentId) => {
    try {
      setLoading(true);
      
      await deleteVehicleDocument(documentId);
      
      // Refresh document list
      const documentsData = await getVehicleDocuments(id);
      setDocuments(documentsData);
    } catch (err) {
      console.error('Error deleting document:', err);
      setError('Failed to delete document. Please try again later.');
    } finally {
      setLoading(false);
    }
  };
  
  const handlePreview = (document) => {
    setPreviewDocument(document);
    setPreviewModalVisible(true);
  };
  
  const getDocumentIcon = (fileType) => {
    if (fileType?.includes('pdf')) return <FilePdfOutlined style={{ fontSize: 18 }} />;
    if (fileType?.includes('image')) return <FileImageOutlined style={{ fontSize: 18 }} />;
    if (fileType?.includes('excel') || fileType?.includes('spreadsheet')) return <FileExcelOutlined style={{ fontSize: 18 }} />;
    if (fileType?.includes('word') || fileType?.includes('document')) return <FileWordOutlined style={{ fontSize: 18 }} />;
    return <FileUnknownOutlined style={{ fontSize: 18 }} />;
  };
  
  const getDocumentTypeTag = (documentType) => {
    let color;
    
    switch (documentType.toLowerCase()) {
      case 'title':
        color = 'red';
        break;
      case 'registration':
        color = 'green';
        break;
      case 'insurance':
        color = 'blue';
        break;
      case 'inspection':
        color = 'orange';
        break;
      case 'service':
        color = 'purple';
        break;
      case 'purchase':
        color = 'cyan';
        break;
      default:
        color = 'default';
    }
    
    return <Tag color={color}>{documentType}</Tag>;
  };
  
  const documentColumns = [
    {
      title: 'Type',
      dataIndex: 'documentType',
      key: 'documentType',
      render: (documentType) => getDocumentTypeTag(documentType)
    },
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (title, record) => (
        <div className="document-title">
          {getDocumentIcon(record.fileType)}
          <Text style={{ marginLeft: 8 }}>{title}</Text>
        </div>
      )
    },
    {
      title: 'Uploaded Date',
      dataIndex: 'uploadDate',
      key: 'uploadDate',
      render: (date) => new Date(date).toLocaleDateString()
    },
    {
      title: 'Size',
      dataIndex: 'fileSize',
      key: 'fileSize',
      render: (size) => {
        const kbSize = size / 1024;
        return kbSize < 1024 ? `${kbSize.toFixed(2)} KB` : `${(kbSize / 1024).toFixed(2)} MB`;
      }
    },
    {
      title: 'Uploaded By',
      dataIndex: 'uploadedBy',
      key: 'uploadedBy',
    },
    {
      title: 'Notes',
      dataIndex: 'notes',
      key: 'notes',
      render: (notes) => notes?.length > 50 ? `${notes.substring(0, 50)}...` : notes
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <div className="document-actions">
          <Tooltip title="Preview">
            <Button 
              type="text" 
              icon={<EyeOutlined />}
              onClick={() => handlePreview(record)}
            />
          </Tooltip>
          <Tooltip title="Download">
            <Button 
              type="text" 
              icon={<DownloadOutlined />}
              href={record.downloadUrl}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Popconfirm
              title="Are you sure you want to delete this document?"
              onConfirm={() => handleDelete(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Button 
                type="text" 
                danger
                icon={<DeleteOutlined />}
              />
            </Popconfirm>
          </Tooltip>
        </div>
      )
    }
  ];

  if (loading && !vehicle) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading documents...</p>
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

  return (
    <div className="vehicle-documents">
      <div className="document-header">
        <Title level={2}>
          <FilePdfOutlined /> Vehicle Documents
        </Title>
        {vehicle && (
          <Title level={4}>
            {vehicle.year} {vehicle.make} {vehicle.model} - Stock #{vehicle.stockNumber}
          </Title>
        )}
      </div>
      
      <Card 
        title={`Documents (${documents?.length || 0})`}
        extra={
          <Button 
            type="primary" 
            icon={<UploadOutlined />}
            onClick={() => setUploadModalVisible(true)}
          >
            Upload Document
          </Button>
        }
      >
        <Table 
          dataSource={documents} 
          columns={documentColumns}
          rowKey="id"
          pagination={documents?.length > 10 ? { pageSize: 10 } : false}
        />
        
        {documents?.length === 0 && (
          <div className="empty-documents">
            <Text>No documents available for this vehicle.</Text>
          </div>
        )}
      </Card>
      
      <Modal
        title="Upload New Document"
        visible={uploadModalVisible}
        onCancel={() => setUploadModalVisible(false)}
        footer={null}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleUpload}
        >
          <Form.Item
            name="file"
            label="Document File"
            rules={[{ required: true, message: 'Please select a file to upload' }]}
          >
            <Dragger 
              name="file"
              multiple={false}
              beforeUpload={() => false} // Prevent auto upload
              maxCount={1}
            >
              <p className="ant-upload-drag-icon">
                <UploadOutlined />
              </p>
              <p className="ant-upload-text">Click or drag file to this area to upload</p>
              <p className="ant-upload-hint">
                Support for PDF, Word, Excel, and image files
              </p>
            </Dragger>
          </Form.Item>
          
          <Form.Item
            name="documentType"
            label="Document Type"
            rules={[{ required: true, message: 'Please select document type' }]}
          >
            <Select placeholder="Select document type">
              <Option value="Title">Title</Option>
              <Option value="Registration">Registration</Option>
              <Option value="Insurance">Insurance</Option>
              <Option value="Inspection">Inspection</Option>
              <Option value="Service">Service Records</Option>
              <Option value="Purchase">Purchase Agreement</Option>
              <Option value="Other">Other</Option>
            </Select>
          </Form.Item>
          
          <Form.Item
            name="title"
            label="Document Title"
            rules={[{ required: true, message: 'Please enter document title' }]}
          >
            <Input placeholder="Enter document title" />
          </Form.Item>
          
          <Form.Item
            name="notes"
            label="Notes"
          >
            <Input.TextArea 
              rows={3}
              placeholder="Add notes about this document (optional)"
            />
          </Form.Item>
          
          <Form.Item>
            <div className="form-actions">
              <Button onClick={() => setUploadModalVisible(false)} style={{ marginRight: 8 }}>
                Cancel
              </Button>
              <Button type="primary" htmlType="submit" loading={loading}>
                Upload
              </Button>
            </div>
          </Form.Item>
        </Form>
      </Modal>
      
      <Modal
        title={previewDocument?.title}
        visible={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setPreviewModalVisible(false)}>
            Close
          </Button>,
          <Button 
            key="download" 
            type="primary" 
            icon={<DownloadOutlined />}
            href={previewDocument?.downloadUrl}
          >
            Download
          </Button>
        ]}
        width={800}
      >
        <div className="document-preview">
          {previewDocument?.fileType?.includes('pdf') ? (
            <iframe 
              src={previewDocument?.previewUrl} 
              width="100%" 
              height="500px" 
              title={previewDocument?.title}
              style={{ border: 'none' }}
            ></iframe>
          ) : previewDocument?.fileType?.includes('image') ? (
            <img 
              src={previewDocument?.previewUrl} 
              alt={previewDocument?.title}
              style={{ maxWidth: '100%' }} 
            />
          ) : (
            <div className="preview-not-available">
              <FileUnknownOutlined style={{ fontSize: 48 }} />
              <p>Preview not available for this file type.</p>
              <p>Please download the file to view it.</p>
            </div>
          )}
        </div>
        
        <Divider />
        
        <div className="document-metadata">
          <p><strong>Document Type:</strong> {previewDocument?.documentType}</p>
          <p><strong>Uploaded On:</strong> {previewDocument && new Date(previewDocument.uploadDate).toLocaleString()}</p>
          {previewDocument?.notes && (
            <>
              <p><strong>Notes:</strong></p>
              <p>{previewDocument.notes}</p>
            </>
          )}
        </div>
      </Modal>
    </div>
  );
};

export default VehicleDocuments;
