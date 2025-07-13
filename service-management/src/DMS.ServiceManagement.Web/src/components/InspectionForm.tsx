import React, { useState, useEffect } from 'react';
import { Box, Typography, Paper, Grid, TextField, Button, FormControl, 
         InputLabel, Select, MenuItem, CircularProgress, Alert, Chip,
         Card, CardContent, CardMedia, IconButton, FormHelperText } from '@mui/material';
import CameraAltIcon from '@mui/icons-material/CameraAlt';
import DeleteIcon from '@mui/icons-material/Delete';
import SaveIcon from '@mui/icons-material/Save';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import { 
    createInspection, 
    uploadInspectionImage, 
    InspectionResult,
    ServiceUrgency,
    InspectionPoint,
    RecommendedService,
    ServiceInspection
} from '../services/inspectionService';
import { 
    validateForm, 
    inspectionFormValidators,
    recommendedServiceValidators 
} from '../utils/validation';
import { 
    ValidationError,
    InspectionFormData,
    InspectionItem as TypedInspectionItem,
    ServiceRecommendation as TypedServiceRecommendation
} from '../utils/types';

// Define inspection point categories
const INSPECTION_CATEGORIES = [
    {
        name: 'Exterior',
        points: ['Body Condition', 'Paint Condition', 'Glass/Mirrors', 'Lights', 'Tires', 'Wheels']
    },
    {
        name: 'Under Hood',
        points: ['Engine Oil', 'Coolant', 'Brake Fluid', 'Power Steering Fluid', 'Battery', 'Air Filter', 'Belts', 'Hoses']
    },
    {
        name: 'Brakes',
        points: ['Front Pads', 'Rear Pads', 'Front Rotors', 'Rear Rotors', 'Parking Brake', 'Brake Lines']
    },
    {
        name: 'Suspension',
        points: ['Shocks/Struts', 'Springs', 'Control Arms', 'Steering Components', 'Alignment']
    },
    {
        name: 'Drivetrain',
        points: ['Transmission Fluid', 'CV Joints/Boots', 'Differential Fluid', 'Driveshaft']
    }
];

interface InspectionFormProps {
    repairOrderId: string;
    vehicleId: string;
    onSave?: () => void;
    onCancel?: () => void;
}

const InspectionForm: React.FC<InspectionFormProps> = ({ 
    repairOrderId, 
    vehicleId, 
    onSave = () => {}, 
    onCancel = () => {}
}) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [formErrors, setFormErrors] = useState<ValidationError<InspectionFormData>>({});
    const [recommendedServiceErrors, setRecommendedServiceErrors] = useState<ValidationError<TypedServiceRecommendation>>({});

    // Initialize inspection points
    const initialInspectionPoints = INSPECTION_CATEGORIES.flatMap(category => 
        category.points.map(point => ({
            id: `${category.name}-${point}`.replace(/\s+/g, '-').toLowerCase(),
            name: point,
            category: category.name,
            result: InspectionResult.NotApplicable,
            notes: '',
            imageUrls: []
        }))
    );

    const [inspectionPoints, setInspectionPoints] = useState<InspectionPoint[]>(initialInspectionPoints);
    const [recommendedServices, setRecommendedServices] = useState<RecommendedService[]>([]);
    const [generalNotes, setGeneralNotes] = useState('');
    
    // State for the new service form
    const [newService, setNewService] = useState<Omit<RecommendedService, 'id'>>({
        description: '',
        urgency: ServiceUrgency.Future,
        estimatedPrice: 0
    });

    // Handle inspection result change
    const handleResultChange = (id: string, result: InspectionResult) => {
        setInspectionPoints(points =>
            points.map(point =>
                point.id === id ? { ...point, result } : point
            )
        );
        // Clear any form errors when the user makes changes
        setFormErrors({});
    };

    // Handle inspection note change
    const handleNotesChange = (id: string, notes: string) => {
        setInspectionPoints(points =>
            points.map(point =>
                point.id === id ? { ...point, notes } : point
            )
        );
    };

    // Handle image upload (mock implementation - in a real app this would upload to a server)
    const handleImageUpload = (id: string, files: FileList | null) => {
        if (!files || files.length === 0) return;
        
        // In a real implementation, you would upload these files to a server
        // For now, we'll create local URLs for demo purposes
        const newImageUrls = Array.from(files).map(file => 
            URL.createObjectURL(file)
        );
        
        setInspectionPoints(points =>
            points.map(point =>
                point.id === id 
                ? { ...point, imageUrls: [...point.imageUrls, ...newImageUrls] } 
                : point
            )
        );
    };

    // Handle image removal
    const handleRemoveImage = (pointId: string, imageUrl: string) => {
        setInspectionPoints(points =>
            points.map(point =>
                point.id === pointId 
                ? { ...point, imageUrls: point.imageUrls.filter(url => url !== imageUrl) } 
                : point
            )
        );
    };    // Validate the new service before adding
    const validateNewService = (): boolean => {
        // Convert ServiceUrgency enum to string type expected by validation
        const serviceToValidate = {
            ...newService,
            urgency: mapUrgencyToType(newService.urgency)
        };
        const errors = validateForm(serviceToValidate as unknown as TypedServiceRecommendation, recommendedServiceValidators);
        setRecommendedServiceErrors(errors);
        return Object.keys(errors).length === 0;
    };

    // Handle adding a new recommended service
    const handleAddService = () => {
        if (!validateNewService()) return;
        
        const service: RecommendedService = {
            ...newService,
            id: `service-${Date.now()}`
        };
        
        setRecommendedServices([...recommendedServices, service]);
        setNewService({
            description: '',
            urgency: ServiceUrgency.Future,
            estimatedPrice: 0
        });
        setRecommendedServiceErrors({});
    };

    // Handle removing a recommended service
    const handleRemoveService = (id: string) => {
        setRecommendedServices(services =>
            services.filter(service => service.id !== id)
        );
    };

    // Validate the entire form
    const validateInspectionForm = (): boolean => {
        // Filter out only the points that were actually inspected
        const completedPoints = inspectionPoints.filter(
            point => point.result !== InspectionResult.NotApplicable
        );
        
        const formData: InspectionFormData = {
            repairOrderId,
            technicianId: '550e8400-e29b-41d4-a716-446655440000', // Placeholder
            type: 'MultiPoint',
            notes: generalNotes,
            inspectionItems: completedPoints.map(point => ({
                id: point.id,
                name: point.name,
                category: point.category,
                status: mapResultToStatus(point.result),
                notes: point.notes,
                images: point.imageUrls
            })),
            recommendations: recommendedServices.map(service => ({
                id: service.id,
                description: service.description,
                urgency: mapUrgencyToType(service.urgency),
                estimatedPrice: service.estimatedPrice
            }))
        };
        
        const errors = validateForm(formData, inspectionFormValidators);
        setFormErrors(errors);
        
        return Object.keys(errors).length === 0;
    };
    
    // Map InspectionResult to status string
    const mapResultToStatus = (result: InspectionResult): 'pass' | 'fail' | 'warning' | '' => {
        switch (result) {
            case InspectionResult.Pass:
                return 'pass';
            case InspectionResult.Fail:
                return 'fail';
            case InspectionResult.Warning:
                return 'warning';
            default:
                return '';
        }
    };
    
    // Map ServiceUrgency to type string
    const mapUrgencyToType = (urgency: ServiceUrgency): 'Critical' | 'Soon' | 'Future' => {
        switch (urgency) {
            case ServiceUrgency.Critical:
                return 'Critical';
            case ServiceUrgency.Soon:
                return 'Soon';
            case ServiceUrgency.Future:
                return 'Future';
            default:
                return 'Future';
        }
    };

    // Handle form submission
    const handleSubmit = async () => {
        try {
            if (!validateInspectionForm()) {
                setError('Please fix the validation errors before submitting.');
                return;
            }
            
            setLoading(true);
            setError(null);
            
            // Filter out only the points that were actually inspected
            const completedPoints = inspectionPoints.filter(
                point => point.result !== InspectionResult.NotApplicable
            );
            
            if (completedPoints.length === 0) {
                setError('At least one inspection point must be completed');
                setLoading(false);
                return;
            }
            
            // Get the current user ID from auth context - would be implemented in a real app
            const technicianId = '550e8400-e29b-41d4-a716-446655440000'; // Placeholder
            
            // Prepare the data to send to the API
            const inspectionData: ServiceInspection = {
                repairOrderId,
                vehicleId,
                technicianId,
                type: 'MultiPoint',
                status: 'Completed',
                startTime: new Date().toISOString(),
                endTime: new Date().toISOString(),
                notes: generalNotes,
                inspectionPoints: completedPoints,
                recommendedServices: recommendedServices
            };
            
            // Call the API to create the inspection
            const result = await createInspection(inspectionData);
            
            // Upload any images that were added during the inspection process
            // Note: In a production app, we would handle this as part of the form submission
            // or potentially use a different approach for image uploads
            const imageUploadPromises: Promise<string>[] = [];
            
            inspectionPoints.forEach(point => {
                if (point.imageUrls && point.imageUrls.length > 0) {
                    // This is where we would upload any images that were captured
                    // For now, this is just placeholder code since we're using object URLs locally
                    // In a real app, we would convert the File objects to FormData and upload them
                    console.log(`Would upload ${point.imageUrls.length} images for point ${point.id}`);
                }
            });
            
            // Wait for all image uploads to complete
            if (imageUploadPromises.length > 0) {
                await Promise.all(imageUploadPromises);
            }
            
            setSuccess('Inspection completed successfully!');
            if (onSave) {
                onSave();
            }
        } catch (err) {
            console.error('Error submitting inspection:', err);
            setError('Failed to submit inspection. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    // Get result icon and color
    const getResultStyle = (result: InspectionResult) => {
        switch (result) {
            case InspectionResult.Pass:
                return { icon: <CheckCircleIcon />, color: 'success.main' };
            case InspectionResult.Warning:
                return { icon: <WarningIcon />, color: 'warning.main' };
            case InspectionResult.Fail:
                return { icon: <ErrorIcon />, color: 'error.main' };
            default:
                return { icon: null, color: 'text.secondary' };
        }
    };

    // Get urgency chip color
    const getUrgencyColor = (urgency: ServiceUrgency) => {
        switch (urgency) {
            case ServiceUrgency.Critical:
                return 'error';
            case ServiceUrgency.Soon:
                return 'warning';
            case ServiceUrgency.Future:
                return 'info';
            default:
                return 'default';
        }
    };

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>
                Vehicle Inspection Form
            </Typography>
            
            <Typography variant="subtitle1" color="textSecondary" gutterBottom>
                Repair Order: {repairOrderId.substring(0, 8)}
            </Typography>
            
            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}
            
            {success && (
                <Alert severity="success" sx={{ mb: 3 }}>
                    {success}
                </Alert>
            )}
            
            <Grid container spacing={3}>
                {/* Inspection Categories */}
                {INSPECTION_CATEGORIES.map(category => (
                    <Grid item xs={12} key={category.name}>
                        <Paper sx={{ p: 2, mb: 2 }}>
                            <Typography variant="h6" sx={{ mb: 2 }}>
                                {category.name}
                            </Typography>
                            
                            <Grid container spacing={2}>
                                {category.points.map(point => {
                                    const pointId = `${category.name}-${point}`.replace(/\s+/g, '-').toLowerCase();
                                    const inspectionPoint = inspectionPoints.find(p => p.id === pointId);
                                    
                                    if (!inspectionPoint) return null;
                                    
                                    const { icon, color } = getResultStyle(inspectionPoint.result);
                                    
                                    return (
                                        <Grid item xs={12} sm={6} md={4} key={pointId}>
                                            <Card variant="outlined">
                                                <CardContent>
                                                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                                                        <Typography variant="subtitle1" sx={{ flexGrow: 1 }}>
                                                            {point}
                                                        </Typography>
                                                        {icon && (
                                                            <Box sx={{ color }} display="flex">
                                                                {icon}
                                                            </Box>
                                                        )}
                                                    </Box>
                                                    
                                                    <FormControl fullWidth variant="outlined" size="small" sx={{ mb: 2 }}>
                                                        <InputLabel>Result</InputLabel>
                                                        <Select
                                                            value={inspectionPoint.result}
                                                            onChange={(e) => handleResultChange(pointId, e.target.value as InspectionResult)}
                                                            label="Result"
                                                        >
                                                            <MenuItem value={InspectionResult.Pass}>Pass</MenuItem>
                                                            <MenuItem value={InspectionResult.Warning}>Warning</MenuItem>
                                                            <MenuItem value={InspectionResult.Fail}>Fail</MenuItem>
                                                            <MenuItem value={InspectionResult.NotApplicable}>N/A</MenuItem>
                                                        </Select>
                                                    </FormControl>
                                                    
                                                    {inspectionPoint.result !== InspectionResult.NotApplicable && (
                                                        <>
                                                            <TextField
                                                                fullWidth
                                                                size="small"
                                                                variant="outlined"
                                                                label="Notes"
                                                                value={inspectionPoint.notes}
                                                                onChange={(e) => handleNotesChange(pointId, e.target.value)}
                                                                sx={{ mb: 2 }}
                                                                error={inspectionPoint.result === InspectionResult.Fail && !inspectionPoint.notes}
                                                                helperText={
                                                                    inspectionPoint.result === InspectionResult.Fail && !inspectionPoint.notes 
                                                                    ? "Notes are required for failed items" 
                                                                    : ""
                                                                }
                                                            />
                                                            
                                                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                                                <Button
                                                                    component="label"
                                                                    variant="outlined"
                                                                    startIcon={<CameraAltIcon />}
                                                                    size="small"
                                                                >
                                                                    Add Photo
                                                                    <input
                                                                        type="file"
                                                                        hidden
                                                                        accept="image/*"
                                                                        onChange={(e) => handleImageUpload(pointId, e.target.files)}
                                                                        multiple
                                                                    />
                                                                </Button>
                                                            </Box>
                                                            
                                                            {inspectionPoint.imageUrls.length > 0 && (
                                                                <Grid container spacing={1}>
                                                                    {inspectionPoint.imageUrls.map((url, index) => (
                                                                        <Grid item xs={6} key={index}>
                                                                            <Box sx={{ position: 'relative' }}>
                                                                                <CardMedia
                                                                                    component="img"
                                                                                    height="80"
                                                                                    image={url}
                                                                                    alt={`${point} image ${index + 1}`}
                                                                                    sx={{ borderRadius: 1 }}
                                                                                />
                                                                                <IconButton
                                                                                    size="small"
                                                                                    sx={{ 
                                                                                        position: 'absolute',
                                                                                        top: 0,
                                                                                        right: 0,
                                                                                        backgroundColor: 'rgba(0,0,0,0.5)',
                                                                                        color: 'white',
                                                                                        '&:hover': {
                                                                                            backgroundColor: 'rgba(0,0,0,0.7)'
                                                                                        }
                                                                                    }}
                                                                                    onClick={() => handleRemoveImage(pointId, url)}
                                                                                >
                                                                                    <DeleteIcon fontSize="small" />
                                                                                </IconButton>
                                                                            </Box>
                                                                        </Grid>
                                                                    ))}
                                                                </Grid>
                                                            )}
                                                        </>
                                                    )}
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                    );
                                })}
                            </Grid>
                        </Paper>
                    </Grid>
                ))}
                
                {/* Recommended Services */}
                <Grid item xs={12}>
                    <Paper sx={{ p: 2 }}>
                        <Typography variant="h6" sx={{ mb: 2 }}>
                            Recommended Services
                        </Typography>
                        
                        <Grid container spacing={2} sx={{ mb: 3 }}>
                            <Grid item xs={12} sm={5}>
                                <TextField
                                    fullWidth
                                    label="Service Description"
                                    variant="outlined"
                                    value={newService.description}
                                    onChange={(e) => setNewService({...newService, description: e.target.value})}
                                    error={!!recommendedServiceErrors.description}
                                    helperText={recommendedServiceErrors.description}
                                />
                            </Grid>
                            <Grid item xs={12} sm={3}>
                                <FormControl fullWidth variant="outlined" error={!!recommendedServiceErrors.urgency}>
                                    <InputLabel>Urgency</InputLabel>
                                    <Select
                                        value={newService.urgency}
                                        onChange={(e) => setNewService({...newService, urgency: e.target.value as ServiceUrgency})}
                                        label="Urgency"
                                    >
                                        <MenuItem value={ServiceUrgency.Critical}>Critical</MenuItem>
                                        <MenuItem value={ServiceUrgency.Soon}>Soon</MenuItem>
                                        <MenuItem value={ServiceUrgency.Future}>Future</MenuItem>
                                    </Select>
                                    {recommendedServiceErrors.urgency && (
                                        <FormHelperText>{recommendedServiceErrors.urgency}</FormHelperText>
                                    )}
                                </FormControl>
                            </Grid>
                            <Grid item xs={12} sm={2}>
                                <TextField
                                    fullWidth
                                    label="Est. Price"
                                    variant="outlined"
                                    type="number"
                                    InputProps={{ startAdornment: '$' }}
                                    value={newService.estimatedPrice}
                                    onChange={(e) => setNewService({...newService, estimatedPrice: parseFloat(e.target.value) || 0})}
                                    error={!!recommendedServiceErrors.estimatedPrice}
                                    helperText={recommendedServiceErrors.estimatedPrice}
                                />
                            </Grid>
                            <Grid item xs={12} sm={2}>
                                <Button
                                    fullWidth
                                    variant="contained"
                                    color="primary"
                                    onClick={handleAddService}
                                    sx={{ height: '100%' }}
                                >
                                    Add
                                </Button>
                            </Grid>
                        </Grid>
                        
                        {formErrors.recommendations && (
                            <Alert severity="error" sx={{ mb: 2 }}>
                                {formErrors.recommendations}
                            </Alert>
                        )}
                        
                        {recommendedServices.length > 0 ? (
                            <Grid container spacing={1}>
                                {recommendedServices.map(service => (
                                    <Grid item xs={12} key={service.id}>
                                        <Paper variant="outlined" sx={{ p: 2, display: 'flex', alignItems: 'center' }}>
                                            <Box sx={{ flexGrow: 1 }}>
                                                <Typography variant="body1">
                                                    {service.description}
                                                </Typography>
                                                <Box sx={{ display: 'flex', alignItems: 'center', mt: 0.5 }}>
                                                    <Chip
                                                        label={service.urgency}
                                                        size="small"
                                                        color={getUrgencyColor(service.urgency)}
                                                        sx={{ mr: 1 }}
                                                    />
                                                    <Typography variant="body2" color="text.secondary">
                                                        Estimated Price: ${service.estimatedPrice.toFixed(2)}
                                                    </Typography>
                                                </Box>
                                            </Box>
                                            <IconButton onClick={() => handleRemoveService(service.id)}>
                                                <DeleteIcon />
                                            </IconButton>
                                        </Paper>
                                    </Grid>
                                ))}
                            </Grid>
                        ) : (
                            <Typography variant="body2" color="text.secondary">
                                No recommended services added yet.
                            </Typography>
                        )}
                    </Paper>
                </Grid>
                
                {/* General Notes */}
                <Grid item xs={12}>
                    <Paper sx={{ p: 2 }}>
                        <Typography variant="h6" sx={{ mb: 2 }}>
                            General Notes
                        </Typography>
                        <TextField
                            fullWidth
                            multiline
                            rows={4}
                            variant="outlined"
                            label="Additional notes about the vehicle condition"
                            value={generalNotes}
                            onChange={(e) => setGeneralNotes(e.target.value)}
                            error={!!formErrors.notes}
                            helperText={formErrors.notes}
                        />
                    </Paper>
                </Grid>
            </Grid>
            
            {Object.keys(formErrors).length > 0 && (
                <Alert severity="error" sx={{ mt: 3, mb: 2 }}>
                    Please fix all validation errors before submitting.
                </Alert>
            )}
            
            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
                <Button
                    variant="outlined"
                    color="secondary"
                    onClick={onCancel}
                    disabled={loading}
                >
                    Cancel
                </Button>
                
                <Button
                    variant="contained"
                    color="primary"
                    startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
                    onClick={handleSubmit}
                    disabled={loading}
                >
                    {loading ? 'Saving...' : 'Complete Inspection'}
                </Button>
            </Box>
        </Box>
    );
};

export default InspectionForm;
