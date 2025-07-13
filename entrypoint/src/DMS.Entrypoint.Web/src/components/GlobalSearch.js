import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  TextField,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
  Typography,
  Box,
  Chip,
  InputAdornment,
  IconButton,
  CircularProgress,
} from '@mui/material';
import {
  Search,
  Close,
  DirectionsCar,
  Person,
  Build,
  Extension,
  TrendingUp,
} from '@mui/icons-material';
import { useQuery } from 'react-query';
import axios from 'axios';
import { debounce } from 'lodash';

const getIconForType = (type) => {
  switch (type) {
    case 'vehicle':
      return <DirectionsCar />;
    case 'customer':
      return <Person />;
    case 'service':
      return <Build />;
    case 'part':
      return <Extension />;
    case 'deal':
      return <TrendingUp />;
    default:
      return <Search />;
  }
};

const getColorForType = (type) => {
  switch (type) {
    case 'vehicle':
      return 'primary';
    case 'customer':
      return 'success';
    case 'service':
      return 'warning';
    case 'part':
      return 'info';
    case 'deal':
      return 'secondary';
    default:
      return 'default';
  }
};

export default function GlobalSearch({ open, onClose }) {
  const [searchQuery, setSearchQuery] = useState('');

  const { data: searchResults, isLoading } = useQuery(
    ['globalSearch', searchQuery],
    async () => {
      if (!searchQuery.trim()) return { results: [] };
      
      const response = await axios.get(`/search?q=${encodeURIComponent(searchQuery)}`);
      return response.data;
    },
    {
      enabled: !!searchQuery.trim(),
      staleTime: 5000,
    }
  );

  const debouncedSearch = debounce((value) => {
    setSearchQuery(value);
  }, 300);

  const handleSearchChange = (event) => {
    debouncedSearch(event.target.value);
  };

  const handleClose = () => {
    setSearchQuery('');
    onClose();
  };

  const allResults = searchResults?.results?.flatMap(service => 
    service.results.map(result => ({
      ...result,
      service: service.service
    }))
  ) || [];

  return (
    <Dialog 
      open={open} 
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { height: '80vh' }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6">Global Search</Typography>
          <IconButton onClick={handleClose}>
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>
      
      <DialogContent>
        <TextField
          autoFocus
          fullWidth
          placeholder="Search across all modules..."
          variant="outlined"
          onChange={handleSearchChange}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search />
              </InputAdornment>
            ),
            endAdornment: isLoading && (
              <InputAdornment position="end">
                <CircularProgress size={20} />
              </InputAdornment>
            ),
          }}
          sx={{ mb: 2 }}
        />

        {searchQuery && (
          <Box>
            {allResults.length === 0 && !isLoading && (
              <Typography color="text.secondary" align="center" sx={{ py: 4 }}>
                No results found for "{searchQuery}"
              </Typography>
            )}

            <List>
              {allResults.map((result, index) => (
                <ListItem 
                  key={`${result.service}-${result.id}-${index}`}
                  sx={{ 
                    cursor: 'pointer',
                    '&:hover': { backgroundColor: 'action.hover' },
                    borderRadius: 1,
                    mb: 1,
                  }}
                >
                  <ListItemAvatar>
                    <Avatar sx={{ backgroundColor: `${getColorForType(result.type)}.main` }}>
                      {getIconForType(result.type)}
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="subtitle1">
                          {result.title}
                        </Typography>
                        <Chip 
                          label={result.service} 
                          size="small" 
                          color={getColorForType(result.type)}
                          variant="outlined"
                        />
                      </Box>
                    }
                    secondary={result.subtitle}
                  />
                </ListItem>
              ))}
            </List>
          </Box>
        )}

        {!searchQuery && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Search sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary">
              Start typing to search across all modules
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Search for vehicles, customers, deals, service appointments, parts, and more...
            </Typography>
          </Box>
        )}
      </DialogContent>
    </Dialog>
  );
}
