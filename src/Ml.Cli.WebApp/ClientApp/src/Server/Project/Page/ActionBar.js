import React from 'react';
import { useHistory } from 'react-router-dom';
import Button from '@axa-fr/react-toolkit-button';
import './Page.scss';
import ExportButton from "./ExportButton";

export const ActionBar = ({ projectId, projectName, isAnnotationClosed, onExport, user }) => {
  const history = useHistory();

  const startTaggingButton = () => {
    const path = `/projects/${projectId}/annotations/start`;
    history.push(path);
  };
  
  if(!projectId){
    return null;
  }
  
  return (
    <div className="ft-actionBar">
      {(isAnnotationClosed) ? null: <Button onClick={startTaggingButton} id="startTagging" name="Start Tagging">
        <span className="af-btn-text">Start Tagging</span>
      </Button>}
      <ExportButton user={user} projectId={projectId} projectName={projectName} onExport={onExport}/>
    </div>
  );
};

export default ActionBar;
