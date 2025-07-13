// @ts-nocheck
import React, { useState, useRef } from 'react';
import { 
  Box, 
  Typography, 
  Button, 
  Dialog, 
  DialogTitle, 
  DialogContent, 
  DialogActions,
  Paper 
} from '@mui/material';
import SignatureCanvas from 'react-signature-canvas';
import { useNotification } from '../../contexts/NotificationContext';

interface DocumentSignerProps {
  documentId: string;
  documentName: string;
  onSignComplete: (signature: string) => Promise<void>;
  fullName?: string;
}

const DocumentSigner: React.FC<DocumentSignerProps> = ({ 
  documentId, 
  documentName, 
  onSignComplete,
  fullName
}) => {
  const [open, setOpen] = useState(false);
  const [signing, setSigning] = useState(false);
  const sigCanvas = useRef<any>(null);
  const { showError, showSuccess, showWarning } = useNotification();

  const handleOpen = () => {
    setOpen(true);
  };

  const handleClose = () => {
    setOpen(false);
  };

  const clearSignature = () => {
    sigCanvas.current.clear();
  };
  const handleSign = async () => {
    if (sigCanvas.current.isEmpty()) {
      showError('Please provide a signature');
      return;
    }

    setSigning(true);
    try {
      const signatureData = sigCanvas.current.toDataURL('image/png');      await onSignComplete(signatureData);
      showSuccess('Document signed successfully!');
      handleClose();
    } catch (error) {
      showError('Error signing document: ' + (error.message || 'Unknown error'));
    } finally {
      setSigning(false);
    }
  };

  return (
    <>
      <Button variant="contained" color="primary" onClick={handleOpen}>
        Sign Document
      </Button>

      <Dialog 
        open={open} 
        onClose={handleClose}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Sign Document: {documentName}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1" gutterBottom>
            By signing below, you acknowledge that you have read and agree to the terms of this document.
          </Typography>
          
          {fullName && (
            <Typography variant="body2" gutterBottom sx={{ mb: 2 }}>
              Signing as: <strong>{fullName}</strong>
            </Typography>
          )}
          
          <Box sx={{ border: '1px solid #ccc', backgroundColor: '#f8f8f8', mb: 2 }}>
            <SignatureCanvas
              ref={sigCanvas}
              canvasProps={{
                width: 550,
                height: 200,
                className: 'signature-canvas'
              }}
              backgroundColor="rgba(255, 255, 255, 0)"
            />
          </Box>
          
          <Box sx={{ display: 'flex', justifyContent: 'flex-start' }}>
            <Button 
              variant="outlined" 
              color="secondary" 
              onClick={clearSignature}
            >
              Clear
            </Button>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={signing}>
            Cancel
          </Button>
          <Button 
            onClick={handleSign} 
            variant="contained" 
            color="primary"
            disabled={signing}
          >
            {signing ? 'Signing...' : 'Sign Document'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default DocumentSigner;
