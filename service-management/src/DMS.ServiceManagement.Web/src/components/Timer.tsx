import React, { useState, useEffect } from 'react';
import { Box, Typography, Button } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';
import RestartAltIcon from '@mui/icons-material/RestartAlt';

interface TimerProps {
    isActive: boolean;
    setIsActive: (active: boolean) => void;
    initialSeconds?: number;
    onTime?: (seconds: number) => void;
    className?: string;
}

const Timer: React.FC<TimerProps> = ({ isActive, setIsActive, initialSeconds = 0, onTime, className }) => {
    const [seconds, setSeconds] = useState(initialSeconds);    useEffect(() => {
        let interval: NodeJS.Timeout | null = null;
        
        if (isActive) {
            interval = setInterval(() => {
                setSeconds(prevSeconds => {
                    const newSeconds = prevSeconds + 1;
                    if (onTime) onTime(newSeconds);
                    return newSeconds;
                });
            }, 1000);
        } else if (!isActive && interval) {
            clearInterval(interval);
        }
        
        return () => {
            if (interval) clearInterval(interval);
        };
    }, [isActive, onTime]);
    
    const formatTime = () => {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const remainingSeconds = seconds % 60;
        
        return [
            hours.toString().padStart(2, '0'),
            minutes.toString().padStart(2, '0'),
            remainingSeconds.toString().padStart(2, '0')
        ].join(':');
    };
    
    const handleReset = () => {
        setIsActive(false);
        setSeconds(0);
    };
      return (
        <Box sx={{ textAlign: 'center' }} className={className}>
            <Typography variant="h3" sx={{ fontFamily: 'monospace', mb: 2 }}>
                {formatTime()}
            </Typography>
            
            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
                <Button
                    variant="contained"
                    color={isActive ? "error" : "success"}
                    onClick={() => setIsActive(!isActive)}
                    startIcon={isActive ? <PauseIcon /> : <PlayArrowIcon />}
                >
                    {isActive ? 'Pause' : 'Start'}
                </Button>
                
                <Button
                    variant="outlined"
                    onClick={handleReset}
                    startIcon={<RestartAltIcon />}
                    disabled={seconds === 0}
                >
                    Reset
                </Button>
            </Box>
        </Box>
    );
};

export default Timer;
