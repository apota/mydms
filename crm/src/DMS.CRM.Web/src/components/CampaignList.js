import React, { useState, useEffect } from 'react';
import { CampaignService } from '../services/api-services';

const CampaignList = () => {
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState('all'); // 'all', 'active', 'upcoming', 'completed'

  useEffect(() => {
    const fetchCampaigns = async () => {
      try {
        setLoading(true);
        let data;
        
        if (filter === 'active') {
          data = await CampaignService.getActive();
        } else {
          // For other filters, we'd typically fetch all and filter client-side
          // In a real implementation, you'd have API endpoints for each filter type
          data = await CampaignService.getAll();
          
          if (filter === 'upcoming') {
            const today = new Date();
            data = data.filter(c => new Date(c.startDate) > today);
          } else if (filter === 'completed') {
            const today = new Date();
            data = data.filter(c => new Date(c.endDate) < today);
          }
        }
        
        setCampaigns(data);
        setLoading(false);
      } catch (err) {
        setError('Failed to fetch campaigns');
        setLoading(false);
        console.error('Error fetching campaigns:', err);
      }
    };

    fetchCampaigns();
  }, [filter]);

  const handleActivate = async (id) => {
    try {
      await CampaignService.activate(id);
      // Refresh the list
      const updatedCampaigns = campaigns.map(campaign => 
        campaign.id === id ? { ...campaign, status: 'Active' } : campaign
      );
      setCampaigns(updatedCampaigns);
    } catch (err) {
      console.error('Error activating campaign:', err);
    }
  };

  const handleDeactivate = async (id) => {
    try {
      await CampaignService.deactivate(id);
      // Refresh the list
      const updatedCampaigns = campaigns.map(campaign => 
        campaign.id === id ? { ...campaign, status: 'Inactive' } : campaign
      );
      setCampaigns(updatedCampaigns);
    } catch (err) {
      console.error('Error deactivating campaign:', err);
    }
  };

  if (loading) return <div className="loading">Loading campaigns...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="campaign-list-container">
      <h2>Marketing Campaigns</h2>
      
      <div className="filter-controls">
        <button 
          className={filter === 'all' ? 'active' : ''}
          onClick={() => setFilter('all')}
        >
          All Campaigns
        </button>
        <button 
          className={filter === 'active' ? 'active' : ''}
          onClick={() => setFilter('active')}
        >
          Active Campaigns
        </button>
        <button 
          className={filter === 'upcoming' ? 'active' : ''}
          onClick={() => setFilter('upcoming')}
        >
          Upcoming Campaigns
        </button>
        <button 
          className={filter === 'completed' ? 'active' : ''}
          onClick={() => setFilter('completed')}
        >
          Completed Campaigns
        </button>
      </div>

      <div className="campaigns-grid">
        {campaigns.map(campaign => (
          <div key={campaign.id} className="campaign-card">
            <h3>{campaign.name}</h3>
            <p className="description">{campaign.description}</p>
            <div className="campaign-dates">
              <span>Start: {new Date(campaign.startDate).toLocaleDateString()}</span>
              <span>End: {new Date(campaign.endDate).toLocaleDateString()}</span>
            </div>
            <div className="campaign-status">
              Status: <span className={`status-${campaign.status.toLowerCase()}`}>{campaign.status}</span>
            </div>
            <div className="campaign-actions">
              <button className="view-btn">View Details</button>
              {campaign.status === 'Active' ? (
                <button 
                  className="deactivate-btn"
                  onClick={() => handleDeactivate(campaign.id)}
                >
                  Deactivate
                </button>
              ) : (
                <button 
                  className="activate-btn"
                  onClick={() => handleActivate(campaign.id)}
                >
                  Activate
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
      
      {campaigns.length === 0 && (
        <div className="no-campaigns">
          No campaigns found for the selected filter.
        </div>
      )}
    </div>
  );
};

export default CampaignList;
