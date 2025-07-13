// @ts-nocheck
import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Button,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  IconButton,
  Card,
  CardContent,
  CardActions,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  FilterList as FilterIcon,
  Visibility as ViewIcon,
  Download as DownloadIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Description as DocumentIcon,
  PictureAsPdf as PdfIcon,
  InsertDriveFile as FileIcon,
  Image as ImageIcon,
} from '@mui/icons-material';
import { fetchDocuments, getSignedDocumentUrl } from '../../services/documentService';
import { DocumentStatus, DocumentCategory } from '../../types/document';

const getStatusColor = (status: DocumentStatus) => {
  switch (status) {
    case DocumentStatus.Draft:
      return 'default';
    case DocumentStatus.PendingSignature:
      return 'warning';
    case DocumentStatus.Signed:
    case DocumentStatus.Completed:
      return 'success';
    case DocumentStatus.Expired:
      return 'error';
    case DocumentStatus.Archived:
      return 'info';
    default:
      return 'default';
  }
};

const getFileIcon = (fileType: string) => {
  const type = fileType.toLowerCase();
  if (type.includes('pdf')) {
    return <PdfIcon />;
  } else if (type.includes('image') || type.includes('jpg') || type.includes('jpeg') || type.includes('png')) {
    return <ImageIcon />;
  } else if (type.includes('doc') || type.includes('office') || type.includes('sheet')) {
    return <DocumentIcon />;
  } else {
    return <FileIcon />;
  }
};

const Documents: React.FC = () => {
  const [categoryFilter, setCategoryFilter] = useState<DocumentCategory | ''>('');
  const [statusFilter, setStatusFilter] = useState<DocumentStatus | ''>('');
  const [searchQuery, setSearchQuery] = useState('');
  const [viewDocumentUrl, setViewDocumentUrl] = useState<string | null>(null);
  const [documentDialog, setDocumentDialog] = useState(false);

  const { data: documents, isLoading } = useQuery(
    ['documents', categoryFilter, statusFilter],
    () => fetchDocuments(categoryFilter || undefined)
  );

  const handleCategoryFilterChange = (event: SelectChangeEvent) => {
    setCategoryFilter(event.target.value as DocumentCategory | '');
  };

  const handleStatusFilterChange = (event: SelectChangeEvent) => {
    setStatusFilter(event.target.value as DocumentStatus | '');
  };

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(event.target.value);
  };

  const handleViewDocument = async (id: string) => {
    try {
      const { url } = await getSignedDocumentUrl(id);
      setViewDocumentUrl(url);
      setDocumentDialog(true);
    } catch (error) {
      console.error('Error getting document URL:', error);
    }
  };

  const handleCloseDocumentDialog = () => {
    setDocumentDialog(false);
    setViewDocumentUrl(null);
  };

  // Filter documents based on search query
  const filteredDocuments = documents?.filter(doc => {
    if (!searchQuery) return true;
    
    const query = searchQuery.toLowerCase();
    return (
      doc.title.toLowerCase().includes(query) ||
      (doc.description && doc.description.toLowerCase().includes(query))
    );
  });

  // Filter documents based on status if statusFilter is set
  const finalFilteredDocs = filteredDocuments?.filter(doc => {
    if (!statusFilter) return true;
    return doc.status === statusFilter;
  });

  return (
    <Box p={3}>
      <Grid container spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Grid item xs>
          <Typography variant="h4" component="h1">
            Documents
          </Typography>
        </Grid>
        <Grid item>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
          >
            Upload Document
          </Button>
        </Grid>
      </Grid>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item>
            <FilterIcon color="action" />
          </Grid>
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Category</InputLabel>
              <Select
                value={categoryFilter}
                label="Category"
                onChange={handleCategoryFilterChange}
              >
                <MenuItem value="">All Categories</MenuItem>
                {Object.values(DocumentCategory).map((category) => (
                  <MenuItem key={category} value={category}>
                    {category}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={statusFilter}
                label="Status"
                onChange={handleStatusFilterChange}
              >
                <MenuItem value="">All Statuses</MenuItem>
                {Object.values(DocumentStatus).map((status) => (
                  <MenuItem key={status} value={status}>
                    {status}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={3}>
            <TextField
              size="small"
              label="Search Documents"
              variant="outlined"
              fullWidth
              value={searchQuery}
              onChange={handleSearchChange}
            />
          </Grid>
        </Grid>
      </Paper>

      {isLoading ? (
        <Box sx={{ width: '100%', mt: 2 }}>
          <LinearProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          {finalFilteredDocs && finalFilteredDocs.length > 0 ? (
            finalFilteredDocs.map((document) => (
              <Grid item xs={12} sm={6} md={4} key={document.id}>
                <Card>
                  <CardContent>
                    <Box display="flex" alignItems="flex-start" mb={1}>
                      <Box mr={2}>{getFileIcon(document.fileType)}</Box>
                      <Box>
                        <Typography variant="h6" component="div" noWrap>
                          {document.title}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {document.category}
                        </Typography>
                        <Chip 
                          label={document.status} 
                          color={getStatusColor(document.status)} 
                          size="small" 
                          sx={{ mt: 1 }} 
                        />
                      </Box>
                    </Box>
                    {document.description && (
                      <Typography variant="body2" sx={{ mt: 1 }}>
                        {document.description}
                      </Typography>
                    )}
                    <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                      Created: {new Date(document.createdAt).toLocaleDateString()}
                    </Typography>
                  </CardContent>
                  <CardActions>
                    <IconButton 
                      aria-label="view" 
                      size="small"
                      onClick={() => handleViewDocument(document.id)}
                    >
                      <ViewIcon />
                    </IconButton>
                    <IconButton aria-label="download" size="small">
                      <DownloadIcon />
                    </IconButton>
                    <IconButton aria-label="edit" size="small">
                      <EditIcon />
                    </IconButton>
                    <IconButton aria-label="delete" size="small">
                      <DeleteIcon />
                    </IconButton>
                  </CardActions>
                </Card>
              </Grid>
            ))
          ) : (
            <Grid item xs={12}>
              <Paper sx={{ p: 3, textAlign: 'center' }}>
                <Typography variant="body1">
                  No documents found matching the current filters.
                </Typography>
              </Paper>
            </Grid>
          )}
        </Grid>
      )}

      {/* Document Preview Dialog */}
      <Dialog
        open={documentDialog}
        onClose={handleCloseDocumentDialog}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>Document Preview</DialogTitle>
        <DialogContent>
          {viewDocumentUrl && (
            <Box sx={{ height: '70vh', width: '100%' }}>
              <iframe 
                src={viewDocumentUrl} 
                title="Document Preview" 
                width="100%" 
                height="100%"
                style={{ border: 'none' }}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDocumentDialog}>Close</Button>
          {viewDocumentUrl && (
            <Button 
              variant="contained" 
              href={viewDocumentUrl} 
              target="_blank"
              rel="noopener noreferrer"
            >
              Open in New Tab
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Documents;
