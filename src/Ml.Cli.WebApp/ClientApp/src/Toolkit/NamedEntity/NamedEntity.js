import React, {useEffect, useState} from 'react';
import Label from './Label';
import './NamedEntity.scss';
import TextAnnotation from './TextAnnotation';
import {GlobalHotKeys} from "react-hotkeys";
import Toolbar from './Toolbar.container';

const initAsync = async (url, setState, state, expectedOutput) => {
  const response = await fetch(url);
  const blob = await response.blob();
  const text = await blob.text();
  setState({...state, text, value: Array.isArray(expectedOutput) ? expectedOutput : [] })
}

const NamedEntity = ({ text= null, labels, onSubmit, placeholder, url, expectedOutput = [] }) => {
  const initialValue = {
    label: labels[0],
    value: [],
    text: '',
    fontSize: 14,
    keepLabels: true
  }
  
  const [state, setState] = useState(initialValue);

  useEffect(() => {
    if(text){
      setState({...initialValue, text});
    } else{
      setState(initialValue);
      initAsync(url, setState, state, expectedOutput);
    }
   
  }, [url, expectedOutput, text]);

  const selectLabel = label => {
    setState({ ...state, label });
  };

  const handleChange = value => {
    setState({ ...state, value });
  };

  const submitAnnotation = () => {
    onSubmit(state.value);
    setState({ ...state, value: [] });
  };
  
  const generateKeyMap = () => {
    let result = {
      submit: 'ctrl+spacebar'
    };
    for(let i = 1; i <= labels.length; i++){
      result[`${i.toString(16)}`] = `${i.toString(16)}`;
    }
    return result;
  };
  
  const generateHandlers = () => {
    let result = {
        submit: () => submitAnnotation()
    };
    for(let i = 1; i <= labels.length; i++){
      result[`${i.toString(16)}`] = () => selectLabel(labels[i - 1]);
    }
    return result;
  };
  
  const keyMap = generateKeyMap();
  const handlers = generateHandlers();

  return (
    <GlobalHotKeys allowChanges={true} keyMap={keyMap} handlers={handlers}>
      <div className="annotationContainer">
        <div className="sticky">
          <Label labels={labels} selectLabel={selectLabel} selectedLabel={state.label} />
        </div>
        <div className="tokenAnnotation-container">
          <TextAnnotation
            text={state.text}
            fontSize={state.fontSize}
            value={state.value}
            keepLabels={state.keepLabels}
            onChange={handleChange}
            getSpan={span => ({
              ...span,
              label: state.label,
            })}
          />
        </div>
        <Toolbar
            state={state}
            setState={setState}
            onSubmit={submitAnnotation}
        />
      </div>
    </GlobalHotKeys>
  );
};

export default NamedEntity;
