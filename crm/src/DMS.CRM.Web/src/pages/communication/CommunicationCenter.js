import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Typography, 
  Paper, 
  Tabs, 
  Tab, 
  Grid, 
  Button, 
  TextField, 
  Divider, 
  List, 
  ListItem, 
  ListItemText, 
  ListItemAvatar, 
  Avatar, 
  Badge, 
  IconButton, 
  Chip, 
  Menu, 
  MenuItem, 
  CircularProgress, 
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  FormHelperText,
} from '@mui/material';
import { 
  Email, 
  Message, 
  Phone, 
  Notifications, 
  Schedule, 
  FilterList, 
  MoreVert, 
  AttachFile, 
  Delete, 
  Reply, 
  Forward, 
  MarkEmailRead, 
  CheckCircle,
  ContactPhone,
  InsertDriveFile,
  AccountCircle,
  Search
} from '@mui/icons-material';
import { format } from 'date-fns';
import { CommunicationService } from '../../services/api-services';

function CommunicationCenter() {
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [messages, setMessages] = useState([]);
  const [templates, setTemplates] = useState([]);
  const [selectedMessage, setSelectedMessage] = useState(null);
  const [composeOpen, setComposeOpen] = useState(false);
  const [composeData, setComposeData] = useState({
    type: 'email',
    to: '',
    subject: '',
    content: '',
    template: '',
    scheduledDate: null
  });
  const [anchorEl, setAnchorEl] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterType, setFilterType] = useState('all');

  // Load initial data
  useEffect(() => {
    const loadCommunications = async () => {
      try {
        setLoading(true);
        // In a real app, you'd fetch from your API
        const sampleMessages = generateSampleMessages();
        setMessages(sampleMessages);
        
        // Load templates
        const templateResponse = await CommunicationService.getTemplates();
        setTemplates(templateResponse || []);
      } catch (err) {
        console.error('Error loading communications:', err);
        setError('Failed to load communications. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    loadCommunications();
  }, []);

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  const handleComposeOpen = (type = 'email') => {
    setComposeData({ ...composeData, type });
    setComposeOpen(true);
  };
  
  const handleComposeClose = () => {
    setComposeOpen(false);
    setComposeData({
      type: 'email',
      to: '',
      subject: '',
      content: '',
      template: '',
      scheduledDate: null
    });
  };
  
  const handleComposeChange = (e) => {
    const { name, value } = e.target;
    setComposeData(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  const handleTemplateSelect = (e) => {
    const templateId = e.target.value;
    if (!templateId) {
      setComposeData(prev => ({
        ...prev,
        template: '',
        subject: '',
        content: ''
      }));
      return;
    }
    
    const selectedTemplate = templates.find(t => t.id === templateId);
    if (selectedTemplate) {
      setComposeData(prev => ({
        ...prev,
        template: templateId,
        subject: selectedTemplate.subject || prev.subject,
        content: selectedTemplate.content || prev.content
      }));
    }
  };
  
  const handleSendMessage = async () => {
    try {
      setLoading(true);
      
      // In a real app, you'd send via your API
      const result = await CommunicationService.sendCustomerMessage({
        type: composeData.type,
        recipient: composeData.to,
        subject: composeData.subject,
        content: composeData.content,
        scheduledDate: composeData.scheduledDate
      });
      
      // Add the sent message to the list
      const newMessage = {
        id: result?.id || Date.now().toString(),
        type: composeData.type,
        recipient: composeData.to,
        subject: composeData.subject,
        content: composeData.content,
        date: composeData.scheduledDate ? new Date(composeData.scheduledDate) : new Date(),
        status: composeData.scheduledDate ? 'scheduled' : 'sent',
        isOutgoing: true
      };
      
      setMessages(prev => [newMessage, ...prev]);
      handleComposeClose();
      
    } catch (err) {
      console.error('Error sending message:', err);
      setError('Failed to send message. Please try again.');
    } finally {
      setLoading(false);
    }
  };
  
  const handleMessageSelect = (message) => {
    setSelectedMessage(message);
  };
  
  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };
  
  const handleMenuClose = () => {
    setAnchorEl(null);
  };
  
  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
  };
  
  const handleFilterChange = (type) => {
    setFilterType(type);
    handleMenuClose();
  };

  // Filter messages based on search and filter type
  const filteredMessages = messages.filter(message => {
    const matchesSearch = 
      searchQuery === '' || 
      message.subject?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      message.content?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      message.recipient?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      message.sender?.toLowerCase().includes(searchQuery.toLowerCase());
      
    const matchesFilter = 
      filterType === 'all' ||
      (filterType === 'email' && message.type === 'email') ||
      (filterType === 'sms' && message.type === 'sms') ||
      (filterType === 'call' && message.type === 'call') ||
      (filterType === 'unread' && !message.read) ||
      (filterType === 'outgoing' && message.isOutgoing) ||
      (filterType === 'incoming' && !message.isOutgoing);
      
    return matchesSearch && matchesFilter;
  });

  // Sample data generator for demo purposes
  const generateSampleMessages = () => {
    return [
      {
        id: '1',
        type: 'email',
        sender: 'john.customer@example.com',
        recipient: 'sales@dealership.com',
        subject: 'Interest in new SUV models',
        content: 'I recently visited your dealership and was interested in the new SUV models. Could you provide more information about financing options?',
        date: new Date(2025, 5, 22, 10, 30),
        read: false,
        isOutgoing: false,
      },
      {
        id: '2',
        type: 'sms',
        sender: '+1234567890',
        recipient: '+1987654321',
        content: 'Your vehicle service is complete and ready for pickup. Total: $349.99',
        date: new Date(2025, 5, 22, 9, 15),
        read: true,
        isOutgoing: true,
      },
      {
        id: '3',
        type: 'call',
        sender: 'Service Department',
        recipient: 'Jane Smith',
        content: 'Called to confirm tomorrow\'s service appointment. Customer confirmed attendance.',
        date: new Date(2025, 5, 21, 16, 45),
        duration: '4:23',
        read: true,
        isOutgoing: true,
      },
      {
        id: '4',
        type: 'email',
        sender: 'sales@dealership.com',
        recipient: 'robert.jones@example.com',
        subject: 'Your recent test drive follow-up',        content: "Thank you for test driving the 2025 Model X with us yesterday. I wanted to follow up to see if you had any questions or if you'd like to discuss financing options.",
        date: new Date(2025, 5, 21, 14, 20),
        read: true,
        isOutgoing: true,
      },
      {
        id: '5',
        type: 'email',
        sender: 'sarah.williams@example.com',
        recipient: 'service@dealership.com',
        subject: 'Issue with recent repair',        content: "I brought my car in last week for a brake service, but I'm still hearing a grinding noise. Can I schedule a follow-up appointment?",
        date: new Date(2025, 5, 21, 11, 5),
        read: false,
        isOutgoing: false,
      },
      {
        id: '6',
        type: 'sms',
        sender: '+1122334455',
        recipient: '+1987654321',        content: "Thanks for the reminder. I'll be there for my 2pm appointment tomorrow.",
        date: new Date(2025, 5, 20, 17, 30),
        read: true,
        isOutgoing: false,
      },
    ];
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">Communication Center</Typography>
        <Box>
          <Button 
            variant="contained" 
            startIcon={<Email />} 
            sx={{ mr: 1 }}
            onClick={() => handleComposeOpen('email')}
          >
            Email
          </Button>
          <Button 
            variant="contained" 
            startIcon={<Message />} 
            sx={{ mr: 1 }}
            onClick={() => handleComposeOpen('sms')}
          >
            SMS
          </Button>
          <Button 
            variant="contained" 
            startIcon={<Phone />}
            onClick={() => handleComposeOpen('call')}
          >
            Log Call
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper sx={{ width: '100%', mb: 4 }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label="communication center tabs">
          <Tab icon={<Notifications />} iconPosition="start" label="Inbox" />
          <Tab icon={<InsertDriveFile />} iconPosition="start" label="Templates" />
          <Tab icon={<Schedule />} iconPosition="start" label="Scheduled" />
          <Tab icon={<ContactPhone />} iconPosition="start" label="Analytics" />
        </Tabs>

        <Box sx={{ p: 3 }}>
          {/* Inbox Tab */}
          {activeTab === 0 && (
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Box sx={{ display: 'flex', mb: 2 }}>
                  <TextField
                    placeholder="Search messages..."
                    variant="outlined"
                    size="small"
                    fullWidth
                    value={searchQuery}
                    onChange={handleSearchChange}
                    InputProps={{
                      startAdornment: <Search color="action" sx={{ mr: 1 }} />,
                    }}
                    sx={{ mr: 2 }}
                  />
                  <Button
                    variant="outlined"
                    startIcon={<FilterList />}
                    onClick={handleMenuOpen}
                  >
                    Filter
                  </Button>
                  <Menu
                    anchorEl={anchorEl}
                    open={Boolean(anchorEl)}
                    onClose={handleMenuClose}
                  >
                    <MenuItem onClick={() => handleFilterChange('all')}>All Messages</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('email')}>Email Only</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('sms')}>SMS Only</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('call')}>Calls Only</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('unread')}>Unread</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('incoming')}>Incoming</MenuItem>
                    <MenuItem onClick={() => handleFilterChange('outgoing')}>Outgoing</MenuItem>
                  </Menu>
                </Box>
              </Grid>
              
              <Grid item xs={12} md={selectedMessage ? 6 : 12}>
                {loading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                    <CircularProgress />
                  </Box>
                ) : filteredMessages.length > 0 ? (
                  <List sx={{ bgcolor: 'background.paper' }}>
                    {filteredMessages.map((message) => (
                      <ListItem 
                        key={message.id} 
                        divider 
                        button 
                        onClick={() => handleMessageSelect(message)}
                        selected={selectedMessage?.id === message.id}
                        sx={{ 
                          bgcolor: !message.read && !message.isOutgoing ? 'action.hover' : 'inherit',
                          "&:hover": {
                            bgcolor: 'action.selected',
                          }
                        }}
                      >
                        <ListItemAvatar>
                          <Badge 
                            color="error" 
                            variant="dot" 
                            invisible={message.read || message.isOutgoing}
                          >
                            <Avatar>
                              {message.type === 'email' ? <Email /> : 
                               message.type === 'sms' ? <Message /> : <Phone />}
                            </Avatar>
                          </Badge>
                        </ListItemAvatar>
                        <ListItemText 
                          primary={
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                              <Typography 
                                variant="subtitle1"
                                sx={{ fontWeight: !message.read && !message.isOutgoing ? 'bold' : 'normal' }}
                              >
                                {message.subject || (message.type === 'sms' ? 'Text Message' : 'Phone Call')}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {format(new Date(message.date), 'MMM d, h:mm a')}
                              </Typography>
                            </Box>
                          }
                          secondary={
                            <Box>
                              <Typography variant="body2">
                                {message.isOutgoing ? 
                                  `To: ${message.recipient}` : 
                                  `From: ${message.sender}`}
                              </Typography>
                              <Typography 
                                variant="body2" 
                                color="text.secondary"
                                sx={{ 
                                  overflow: 'hidden',
                                  textOverflow: 'ellipsis',
                                  display: '-webkit-box',
                                  WebkitLineClamp: 1,
                                  WebkitBoxOrient: 'vertical',
                                }}
                              >
                                {message.content}
                              </Typography>
                            </Box>
                          }
                        />
                        <Box>
                          <Chip 
                            size="small" 
                            label={message.isOutgoing ? 'Sent' : 'Received'} 
                            color={message.isOutgoing ? 'primary' : 'default'}
                          />
                        </Box>
                      </ListItem>
                    ))}
                  </List>
                ) : (
                  <Box sx={{ p: 3, textAlign: 'center' }}>
                    <Typography>No messages match your search criteria.</Typography>
                  </Box>
                )}
              </Grid>
              
              {selectedMessage && (
                <Grid item xs={12} md={6}>
                  <Paper sx={{ p: 2, height: '100%' }} variant="outlined">
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Avatar sx={{ mr: 2 }}>
                          {selectedMessage.type === 'email' ? <Email /> : 
                           selectedMessage.type === 'sms' ? <Message /> : <Phone />}
                        </Avatar>
                        <Box>
                          <Typography variant="h6">
                            {selectedMessage.subject || (
                              selectedMessage.type === 'sms' ? 'Text Message' : 'Phone Call Log'
                            )}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {format(new Date(selectedMessage.date), 'PPpp')}
                          </Typography>
                        </Box>
                      </Box>
                      <IconButton onClick={() => setSelectedMessage(null)}>
                        <MoreVert />
                      </IconButton>
                    </Box>
                    
                    <Divider sx={{ mb: 2 }} />
                    
                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2">
                        <strong>
                          {selectedMessage.isOutgoing ? 'To: ' : 'From: '}
                        </strong>
                        {selectedMessage.isOutgoing ? selectedMessage.recipient : selectedMessage.sender}
                      </Typography>
                      {selectedMessage.type === 'call' && selectedMessage.duration && (
                        <Typography variant="body2">
                          <strong>Duration: </strong>{selectedMessage.duration}
                        </Typography>
                      )}
                    </Box>
                    
                    <Box sx={{ mb: 3 }}>
                      <Typography variant="body1" sx={{ whiteSpace: 'pre-line' }}>
                        {selectedMessage.content}
                      </Typography>
                    </Box>
                    
                    <Divider sx={{ mb: 2 }} />
                    
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                      {selectedMessage.type !== 'call' && (
                        <>
                          <Button 
                            variant="outlined" 
                            startIcon={<Reply />}
                            sx={{ mr: 1 }}
                            onClick={() => {
                              handleComposeOpen(selectedMessage.type);
                              setComposeData({
                                ...composeData,
                                type: selectedMessage.type,
                                to: selectedMessage.isOutgoing ? selectedMessage.recipient : selectedMessage.sender,
                                subject: selectedMessage.subject ? `Re: ${selectedMessage.subject}` : '',
                              });
                            }}
                          >
                            Reply
                          </Button>
                          <Button 
                            variant="outlined" 
                            startIcon={<Forward />}
                            sx={{ mr: 1 }}
                          >
                            Forward
                          </Button>
                        </>
                      )}
                      {!selectedMessage.read && !selectedMessage.isOutgoing && (
                        <Button 
                          variant="outlined" 
                          startIcon={<MarkEmailRead />}
                          sx={{ mr: 1 }}
                          onClick={() => {
                            setMessages(prev => prev.map(m => 
                              m.id === selectedMessage.id ? {...m, read: true} : m
                            ));
                            setSelectedMessage(prev => ({...prev, read: true}));
                          }}
                        >
                          Mark as Read
                        </Button>
                      )}
                      <Button 
                        variant="outlined" 
                        color="error"
                        startIcon={<Delete />}
                        onClick={() => {
                          setMessages(prev => prev.filter(m => m.id !== selectedMessage.id));
                          setSelectedMessage(null);
                        }}
                      >
                        Delete
                      </Button>
                    </Box>
                  </Paper>
                </Grid>
              )}
            </Grid>
          )}

          {/* Templates Tab */}
          {activeTab === 1 && (
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">Communication Templates</Typography>
                <Button variant="contained">Create Template</Button>
              </Box>
              
              <Grid container spacing={3}>
                {templates.length > 0 ? (
                  templates.map((template) => (
                    <Grid item xs={12} md={6} key={template.id}>
                      <Paper variant="outlined" sx={{ p: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                          <Typography variant="h6">{template.name}</Typography>
                          <Chip 
                            label={template.type} 
                            color={template.type === 'email' ? 'primary' : 'secondary'} 
                            size="small" 
                          />
                        </Box>
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          Subject: {template.subject || 'N/A'}
                        </Typography>
                        <Typography 
                          variant="body2" 
                          sx={{ 
                            mb: 2,
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            display: '-webkit-box',
                            WebkitLineClamp: 3,
                            WebkitBoxOrient: 'vertical',
                          }}
                        >
                          {template.content}
                        </Typography>
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                          <Button 
                            variant="outlined" 
                            size="small" 
                            sx={{ mr: 1 }}
                            onClick={() => {
                              handleComposeOpen(template.type);
                              setComposeData({
                                ...composeData,
                                type: template.type,
                                subject: template.subject || '',
                                content: template.content || '',
                                template: template.id
                              });
                            }}
                          >
                            Use Template
                          </Button>
                          <Button variant="outlined" size="small">Edit</Button>
                        </Box>
                      </Paper>
                    </Grid>
                  ))
                ) : (
                  <Grid item xs={12}>
                    <Alert severity="info">
                      No templates found. Create templates to streamline your communications.
                    </Alert>
                  </Grid>
                )}
                
                {/* Add some sample templates for the demo */}
                <Grid item xs={12} md={6}>
                  <Paper variant="outlined" sx={{ p: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="h6">Service Reminder</Typography>
                      <Chip label="email" color="primary" size="small" />
                    </Box>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Subject: Your upcoming service appointment
                    </Typography>
                    <Typography 
                      variant="body2" 
                      sx={{ 
                        mb: 2,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        display: '-webkit-box',
                        WebkitLineClamp: 3,
                        WebkitBoxOrient: 'vertical',
                      }}
                    >
                      Dear {'{Customer Name}'},
                      
                      This is a friendly reminder about your upcoming service appointment scheduled for {'{Appointment Date}'} at {'{Time}'}.
                      
                      Please bring your {'{Vehicle Make/Model}'} to our service center at {'{Dealership Address}'}.
                      
                      If you need to reschedule, please contact us at {'{Phone Number}'}.
                      
                      Thank you for choosing our dealership!
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                      <Button 
                        variant="outlined" 
                        size="small" 
                        sx={{ mr: 1 }}
                        onClick={() => {
                          handleComposeOpen('email');
                          setComposeData({
                            ...composeData,
                            type: 'email',
                            subject: 'Your upcoming service appointment',
                            content: `Dear {Customer Name},

This is a friendly reminder about your upcoming service appointment scheduled for {Appointment Date} at {Time}.

Please bring your {Vehicle Make/Model} to our service center at {Dealership Address}.

If you need to reschedule, please contact us at {Phone Number}.

Thank you for choosing our dealership!`
                          });
                        }}
                      >
                        Use Template
                      </Button>
                      <Button variant="outlined" size="small">Edit</Button>
                    </Box>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Paper variant="outlined" sx={{ p: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="h6">Service Complete</Typography>
                      <Chip label="sms" color="secondary" size="small" />
                    </Box>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Subject: N/A
                    </Typography>
                    <Typography 
                      variant="body2" 
                      sx={{ 
                        mb: 2,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        display: '-webkit-box',
                        WebkitLineClamp: 3,
                        WebkitBoxOrient: 'vertical',
                      }}
                    >
                      {'{Customer Name}'}, your {'{Vehicle Make/Model}'} is ready for pickup at our service center. Total: ${'{Amount}'} - {'{Dealership Name}'}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                      <Button 
                        variant="outlined" 
                        size="small" 
                        sx={{ mr: 1 }}
                        onClick={() => {
                          handleComposeOpen('sms');
                          setComposeData({
                            ...composeData,
                            type: 'sms',
                            content: '{Customer Name}, your {Vehicle Make/Model} is ready for pickup at our service center. Total: ${Amount} - {Dealership Name}'
                          });
                        }}
                      >
                        Use Template
                      </Button>
                      <Button variant="outlined" size="small">Edit</Button>
                    </Box>
                  </Paper>
                </Grid>
              </Grid>
            </Box>
          )}

          {/* Scheduled Tab */}
          {activeTab === 2 && (
            <Box>
              <Typography variant="h6" gutterBottom>Scheduled Communications</Typography>
              <Alert severity="info" sx={{ mb: 3 }}>
                Schedule communications to be sent automatically at specified dates and times.
              </Alert>
              
              <List>
                {messages.filter(m => m.status === 'scheduled').length > 0 ? (
                  messages
                    .filter(m => m.status === 'scheduled')
                    .map((message) => (
                      <ListItem 
                        key={message.id} 
                        divider
                        secondaryAction={
                          <IconButton edge="end" aria-label="cancel">
                            <Delete />
                          </IconButton>
                        }
                      >
                        <ListItemAvatar>
                          <Avatar>
                            {message.type === 'email' ? <Email /> : 
                             message.type === 'sms' ? <Message /> : <Phone />}
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText 
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <Typography variant="subtitle1">
                                {message.subject || 'No subject'}
                              </Typography>
                              <Chip 
                                label="Scheduled"
                                color="info"
                                size="small"
                                sx={{ ml: 1 }}
                              />
                            </Box>
                          }
                          secondary={
                            <>
                              <Typography variant="body2">
                                To: {message.recipient}
                              </Typography>
                              <Typography 
                                variant="body2" 
                                color="text.secondary"
                              >
                                Scheduled for: {format(new Date(message.date), 'PPp')}
                              </Typography>
                            </>
                          }
                        />
                      </ListItem>
                    ))
                ) : (
                  <Box sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                      No scheduled communications found.
                    </Typography>
                    <Button 
                      variant="contained" 
                      sx={{ mt: 2 }}
                      onClick={() => {
                        handleComposeOpen('email');
                        setComposeData({
                          ...composeData,
                          scheduledDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().substring(0, 16)
                        });
                      }}
                    >
                      Schedule a Communication
                    </Button>
                  </Box>
                )}
              </List>
            </Box>
          )}

          {/* Analytics Tab */}
          {activeTab === 3 && (
            <Box>
              <Typography variant="h6" gutterBottom>Communication Analytics</Typography>
              <Alert severity="info" sx={{ mb: 3 }}>
                View insights about your communications with customers.
              </Alert>
              
              <Grid container spacing={3}>
                <Grid item xs={12} sm={6} md={3}>
                  <Paper sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h3">87%</Typography>
                    <Typography variant="body2" color="text.secondary">Email Open Rate</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Paper sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h3">65%</Typography>
                    <Typography variant="body2" color="text.secondary">SMS Response Rate</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Paper sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h3">432</Typography>
                    <Typography variant="body2" color="text.secondary">Communications This Month</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Paper sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h3">24min</Typography>
                    <Typography variant="body2" color="text.secondary">Avg Response Time</Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 2 }}>Communication Volume by Type</Typography>
                  <Paper sx={{ p: 2, height: '300px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                      [Communication Volume Chart - Visual representation would be implemented here]
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 2 }}>Response Time Trends</Typography>
                  <Paper sx={{ p: 2, height: '250px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                      [Response Time Chart - Visual representation would be implemented here]
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 2 }}>Customer Engagement Score</Typography>
                  <Paper sx={{ p: 2, height: '250px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                      [Engagement Score Chart - Visual representation would be implemented here]
                    </Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Box>
          )}
        </Box>
      </Paper>

      {/* Compose Dialog */}
      <Dialog 
        open={composeOpen} 
        onClose={handleComposeClose}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>
          {composeData.type === 'email' ? 'Compose Email' : 
           composeData.type === 'sms' ? 'Compose SMS' : 'Log Phone Call'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            {/* To Field */}
            <Grid item xs={12}>
              <TextField
                label="To"
                name="to"
                value={composeData.to}
                onChange={handleComposeChange}
                fullWidth
                required
              />
            </Grid>
            
            {/* Subject (Email Only) */}
            {composeData.type === 'email' && (
              <Grid item xs={12}>
                <TextField
                  label="Subject"
                  name="subject"
                  value={composeData.subject}
                  onChange={handleComposeChange}
                  fullWidth
                  required
                />
              </Grid>
            )}
            
            {/* Template Selection */}
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Use Template</InputLabel>
                <Select
                  value={composeData.template}
                  onChange={handleTemplateSelect}
                  label="Use Template"
                >
                  <MenuItem value="">
                    <em>None</em>
                  </MenuItem>
                  <MenuItem value="template1">Service Reminder</MenuItem>
                  <MenuItem value="template2">Service Complete</MenuItem>
                  <MenuItem value="template3">Follow-up After Visit</MenuItem>
                  <MenuItem value="template4">New Vehicle Promotion</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            
            {/* Content/Message */}
            <Grid item xs={12}>
              <TextField
                label={composeData.type === 'call' ? 'Call Notes' : 'Message'}
                name="content"
                value={composeData.content}
                onChange={handleComposeChange}
                fullWidth
                required
                multiline
                rows={composeData.type === 'sms' ? 4 : 8}
              />
            </Grid>
            
            {/* Scheduled Date/Time */}
            <Grid item xs={12}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <FormControl sx={{ flexGrow: 1, mr: 2 }}>
                  <TextField
                    label="Schedule for"
                    type="datetime-local"
                    name="scheduledDate"
                    value={composeData.scheduledDate || ''}
                    onChange={handleComposeChange}
                    InputLabelProps={{
                      shrink: true,
                    }}
                  />
                  <FormHelperText>
                    Leave blank to send immediately
                  </FormHelperText>
                </FormControl>
                
                {composeData.type === 'email' && (
                  <Button 
                    variant="outlined" 
                    startIcon={<AttachFile />}
                  >
                    Attach File
                  </Button>
                )}
              </Box>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleComposeClose}>Cancel</Button>
          <Button 
            variant="contained" 
            onClick={handleSendMessage}
            disabled={!composeData.to || (!composeData.content && composeData.type !== 'email') || (composeData.type === 'email' && !composeData.subject)}
            endIcon={composeData.scheduledDate ? <Schedule /> : <CheckCircle />}
          >
            {composeData.scheduledDate ? 'Schedule' : composeData.type === 'call' ? 'Save' : 'Send'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default CommunicationCenter;
