import React from 'react';
import { scaleBy } from './Cropping';
import { GlobalHotKeys } from 'react-hotkeys';
import cuid from 'cuid';
import Toolbar, {ToolbarButtonContainer, ToolbarButton, ToolbarSwitchButton} from '../Toolbar';

const getFileExtension = filename => {
  if (!filename) return '';
  if (filename.startsWith('data:')) {
    return filename.split(',')[0].split('/')[1].split(';')[0];
  }
  return filename.split('.').pop().split('?')[0];
};

const getShapeWithFocus = shapes => {
  for (let i = 0; i < shapes.length; i++) {
    if (shapes[i].focus) {
      return shapes[i];
    }
  }
  return null;
};
const _isDeleteDisabled = shapes => {
  const shape = getShapeWithFocus(shapes);
  return !shape;
};

const ToolbarContainer = ({ setState, state, fitImage, onSubmit, image, initShape, labels }) => {
  const isSubmitDisabled = !image;
  const isDeleteDisabled = _isDeleteDisabled(state.shapes);

  const onDelete = (e) => {
    e.preventDefault();
    const shape = getShapeWithFocus(state.shapes);
    const newShapes = [...state.shapes];
    if (shape) {
      const index = newShapes.indexOf(shape);
      if (index > -1) {
        newShapes.splice(index, 1);
      }
      document.body.style.cursor = '';
    }
    setState({ ...state, shapes: newShapes });
  };

  const handleSubmit = e => {
    e.preventDefault();
    if (isSubmitDisabled) {
      return;
    }
    const inputImageProperties = {
      height: 0,
      width: 0,
    };
    if (image) {
      inputImageProperties.height = image.height;
      inputImageProperties.width = image.width;
    }
    const boundingBoxes = state.shapes.map(shape => {
      return {
        label: labels.find(l => l.id === shape.labelId).label,
        height: Math.round(Math.abs(shape.begin.y - shape.end.y)),
        left: Math.round(Math.min(shape.begin.x, shape.end.x)),
        top: Math.round(Math.min(shape.begin.y, shape.end.y)),
        width: Math.round(Math.abs(shape.begin.x - shape.end.x)),
      };
    });

    const result = {
      width: inputImageProperties.width,
      height: inputImageProperties.height,
      type: getFileExtension(image.currentSrc),
      labels: {
        boundingBoxes,
      },
    };
    onSubmit(result);
  };

  const onZoomIn = (e) => {
    e.preventDefault();
    setState({ ...state, stageScale: state.stageScale * scaleBy });
  };

  const onZoomOut = (e) => {
    e.preventDefault();
    setState({ ...state, stageScale: state.stageScale / scaleBy });
  };

  const handleFitImage = (e) => {
    e.preventDefault();
    setState({ ...state, ...fitImage() });
  };

  const moveImage = (e) => {
    e.preventDefault();
    setState({ ...state, moveImageActive:!state.moveImageActive });
  };

  const selectAll = () => {
    let height = 0;
    let width = 0;
    if (image) {
      height = image.height;
      width = image.width;
    }
    const { newTempState, newShape } = initShape(state, {
      begin: { x: 0, y: 0 },
      end: { x: width, y: height },
      id: cuid(),
      focus: true,
      labelId: state.currentLabelId,
    });
    setState({ ...newTempState, shapes: [...state.shapes, newShape] });
  };

  const keepAnnotationToggle = () => {
    setState({ ...state, keepAnnotation: !state.keepAnnotation });
  };

  const keyMap = {
    Submit: 'ctrl+spacebar',
    Delete: 'backspace',
    ZoomIn: 'z',
    ZoomOut: 'o',
    FitImage: 'f',
    SelectAll: 'a',
  };

  const handlers = {
    Submit: handleSubmit,
    Delete: onDelete,
    ZoomIn: onZoomIn,
    ZoomOut: onZoomOut,
    FitImage: handleFitImage,
    SelectAll: selectAll,
  };

  return (
    <GlobalHotKeys allowChanges={true} keyMap={keyMap} handlers={handlers}>
      <Toolbar isSubmitDisabled={isSubmitDisabled} onSubmit={handleSubmit}>
        <ToolbarButtonContainer>
          <ToolbarSwitchButton
            id="keepAnnotation"
            checked={state.keepAnnotation}
            onChange={keepAnnotationToggle}
            label="Keep Annotation"
          />
        </ToolbarButtonContainer>
        <ToolbarButtonContainer>
          <ToolbarButton
            title="Raccourci : Delete"
            onClick={onDelete}
            disabled={isDeleteDisabled}
            icon="trash"
            label="Delete"
          />
          <ToolbarButton title="Raccourci : Z" onClick={onZoomIn} icon="zoom-in" label="Zoom In" />
          <ToolbarButton title="Raccourci : 0" onClick={onZoomOut} icon="zoom-out" label="Zoom Out" />
          <ToolbarButton title="Raccourci : F" onClick={handleFitImage} icon="resize-full" label="Fit Image" />
          <ToolbarButton title="Raccourci : A" onClick={selectAll} icon="picture" label="Select All" />
          <ToolbarButton onClick={moveImage} icon="move" label="Move" toggle={state.moveImageActive} />
        </ToolbarButtonContainer>
      </Toolbar>
    </GlobalHotKeys>
  );
};

export default React.memo(ToolbarContainer);
