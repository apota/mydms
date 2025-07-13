import React, { useState, useRef, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  Typography,
  Avatar,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  IconButton,
  Chip,
  Paper,
  Divider,
} from '@mui/material';
import {
  Close,
  Send,
  SmartToy,
  Person,
  AutoAwesome,
  TrendingUp,
  DirectionsCar,
  Build,
} from '@mui/icons-material';
import { useQuery, useMutation } from 'react-query';
import axios from 'axios';
import { useAuth } from '../contexts/AuthContext';

const sampleQueries = [
  { text: "Show me vehicles over 60 days old", icon: <DirectionsCar /> },
  { text: "What's my sales performance this month?", icon: <TrendingUp /> },
  { text: "Schedule a service appointment", icon: <Build /> },
  { text: "Find customer John Smith's history", icon: <Person /> },
];

export default function AIAssistant({ open, onClose }) {
  const [message, setMessage] = useState('');
  const [conversation, setConversation] = useState([
    {
      id: 1,
      sender: 'rudy',
      message: "Hi! I'm Rudy, your AI assistant. I can help you with inventory management, sales tracking, customer information, and much more. What would you like to know?",
      timestamp: new Date(),
    }
  ]);
  
  const messagesEndRef = useRef(null);
  const { user } = useAuth();

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [conversation]);

  const sendMessageMutation = useMutation(
    async (messageText) => {
      const response = await axios.post('/ai/chat', {
        message: messageText,
        userId: user.id,
        conversationId: 'main', // In a real app, this would be unique per conversation
      });
      return response.data;
    },
    {
      onSuccess: (data) => {
        setConversation(prev => [
          ...prev,
          {
            id: Date.now(),
            sender: 'rudy',
            message: data.response,
            timestamp: new Date(),
            actions: data.actions, // Any suggested actions
          }
        ]);
      },
      onError: () => {
        setConversation(prev => [
          ...prev,
          {
            id: Date.now(),
            sender: 'rudy',
            message: "I'm sorry, I'm having trouble processing that request right now. Please try again.",
            timestamp: new Date(),
          }
        ]);
      }
    }
  );

  const handleSendMessage = () => {
    if (!message.trim()) return;

    const userMessage = {
      id: Date.now(),
      sender: 'user',
      message: message.trim(),
      timestamp: new Date(),
    };

    setConversation(prev => [...prev, userMessage]);
    sendMessageMutation.mutate(message.trim());
    setMessage('');
  };

  const handleSampleQuery = (query) => {
    setMessage(query);
    setTimeout(() => handleSendMessage(), 100);
  };

  const handleKeyPress = (event) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      handleSendMessage();
    }
  };

  const formatTimestamp = (timestamp) => {
    return new Date(timestamp).toLocaleTimeString([], { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  };

  return (
    <Dialog 
      open={open} 
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { height: '80vh', display: 'flex', flexDirection: 'column' }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Avatar sx={{ backgroundColor: 'secondary.main' }}>
              <SmartToy />
            </Avatar>
            <Box>
              <Typography variant="h6">Rudy - AI Assistant</Typography>
              <Typography variant="caption" color="text.secondary">
                Always here to help with your DMS needs
              </Typography>
            </Box>
          </Box>
          <IconButton onClick={onClose}>
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>
      
      <DialogContent sx={{ flex: 1, display: 'flex', flexDirection: 'column', p: 0 }}>
        {/* Sample Queries */}
        {conversation.length === 1 && (
          <Box sx={{ p: 2, backgroundColor: 'grey.50' }}>
            <Typography variant="subtitle2" gutterBottom>
              Try asking me about:
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
              {sampleQueries.map((query, index) => (
                <Chip
                  key={index}
                  icon={query.icon}
                  label={query.text}
                  variant="outlined"
                  clickable
                  onClick={() => handleSampleQuery(query.text)}
                  size="small"
                />
              ))}
            </Box>
          </Box>
        )}

        {/* Messages */}
        <Box sx={{ flex: 1, overflow: 'auto', p: 2 }}>
          {conversation.map((msg) => (
            <Box key={msg.id} sx={{ mb: 2 }}>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  alignItems: 'flex-start',
                  gap: 1,
                }}
              >
                {msg.sender === 'rudy' && (
                  <Avatar sx={{ backgroundColor: 'secondary.main', width: 32, height: 32 }}>
                    <SmartToy sx={{ fontSize: 16 }} />
                  </Avatar>
                )}
                
                <Paper
                  sx={{
                    p: 2,
                    maxWidth: '70%',
                    backgroundColor: msg.sender === 'user' ? 'primary.main' : 'grey.100',
                    color: msg.sender === 'user' ? 'white' : 'text.primary',
                  }}
                >
                  <Typography variant="body1">{msg.message}</Typography>
                  <Typography 
                    variant="caption" 
                    sx={{ 
                      display: 'block', 
                      mt: 1, 
                      opacity: 0.7 
                    }}
                  >
                    {formatTimestamp(msg.timestamp)}
                  </Typography>
                </Paper>

                {msg.sender === 'user' && (
                  <Avatar sx={{ backgroundColor: 'primary.main', width: 32, height: 32 }}>
                    <Person sx={{ fontSize: 16 }} />
                  </Avatar>
                )}
              </Box>

              {/* Actions */}
              {msg.actions && msg.actions.length > 0 && (
                <Box sx={{ mt: 1, ml: 5 }}>
                  {msg.actions.map((action, index) => (
                    <Button
                      key={index}
                      variant="outlined"
                      size="small"
                      startIcon={<AutoAwesome />}
                      sx={{ mr: 1, mb: 1 }}
                      onClick={() => {
                        // Handle action click
                        console.log('Action clicked:', action);
                      }}
                    >
                      {action.label}
                    </Button>
                  ))}
                </Box>
              )}
            </Box>
          ))}
          
          {sendMessageMutation.isLoading && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Avatar sx={{ backgroundColor: 'secondary.main', width: 32, height: 32 }}>
                <SmartToy sx={{ fontSize: 16 }} />
              </Avatar>
              <Paper sx={{ p: 2, backgroundColor: 'grey.100' }}>
                <Typography variant="body1">Rudy is thinking...</Typography>
              </Paper>
            </Box>
          )}
          
          <div ref={messagesEndRef} />
        </Box>
      </DialogContent>

      <Divider />
      
      <DialogActions sx={{ p: 2 }}>
        <TextField
          fullWidth
          placeholder="Ask Rudy anything about your dealership..."
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          multiline
          maxRows={3}
          variant="outlined"
          InputProps={{
            endAdornment: (
              <IconButton 
                color="primary" 
                onClick={handleSendMessage}
                disabled={!message.trim() || sendMessageMutation.isLoading}
              >
                <Send />
              </IconButton>
            ),
          }}
        />
      </DialogActions>
    </Dialog>
  );
}
