import New from './New';
import {rules} from './New.validation.rules';
import { withRouter } from 'react-router-dom';
import { computeInitialStateErrorMessage, genericHandleChange } from '../../../validation.generic';
import {
  NAME,
  TYPE,
  CLASSIFICATION,
  GROUP,
  DATASETS_IMPORT,
  MSG_REQUIRED,
  MSG_DATASET_NAME_ALREADY_EXIST,
  IMPORTED_DATASET_NAME
} from './constants';
import React, { useReducer } from 'react';
import {fetchCreateDataset, fetchDatasets, fetchImportedDatasets} from "../../Dataset.service";
import {resilienceStatus, withResilience} from "../../../shared/Resilience";
import withCustomFetch from "../../../withCustomFetch";
import compose from "../../../compose";
import {fetchGroups} from "../../../Group/Group.service";
import {telemetryEvents, withTelemetry} from "../../../Telemetry";
import {withEnvironment} from "../../../EnvironmentProvider";

const errorList = fields => Object.keys(fields).filter(key => setErrorMessage(key)(fields));

const setErrorMessage = key => fields => (fields[key].message && fields[key].message !== null);

const getImportedDatasetsByGroupId = (groups, groupId, importedDatasets) => {
  const group = groups.find(element => element.id === groupId);
  if(typeof group === 'undefined') return [];
  return importedDatasets.filter(dts => dts.startsWith(group.name));
}

export const preInitState = {
  hasSubmit: false,
  status: resilienceStatus.LOADING,
  groups : [],
  datasets : [],
  importedDatasets : [],
  optionsDatasets : [],
  fields: {
    [NAME]: { name: NAME, value: '', message: MSG_REQUIRED },
    [GROUP]: { name: GROUP, value: '', message: MSG_REQUIRED },
    [TYPE]: { name: TYPE, value: '', message: MSG_REQUIRED },
    [CLASSIFICATION]: {
      name: CLASSIFICATION,
      value: '',
      message: MSG_REQUIRED,
    },
    [DATASETS_IMPORT]: {name: DATASETS_IMPORT, isChecked: true, message: "", isVisible:false},
    [IMPORTED_DATASET_NAME]: {name: IMPORTED_DATASET_NAME, value: '', disabled: true, message: MSG_REQUIRED, isVisible:false}
  },
};

export const initState = computeInitialStateErrorMessage(preInitState, rules);

export const reducer = (state, action) => {
  switch (action.type) {
    case 'init':{
        const { datasets, groups, importedDatasets, status } = action.data;
        return {
          ...state,
          status,
          datasets,
          groups,
          importedDatasets
        };
      }
    case 'onChange': {
      const event = action.event;
      let newField = genericHandleChange(rules, state.fields, event);
      let optionsDatasets = state.optionsDatasets;
      const name = event.name;
      if(NAME === name){
        if(state.datasets.find(dataset => dataset.name.toLocaleLowerCase() === event.value.toLocaleLowerCase())) {
          newField = {
            ...newField,
            [name]: {
              ...newField[name],
              message: MSG_DATASET_NAME_ALREADY_EXIST
            }};
        }
      }
      else if(DATASETS_IMPORT === name){
        const fields = state.fields;
        const isChecked = !fields[DATASETS_IMPORT].isChecked;
        newField = {
          ...newField,
          [DATASETS_IMPORT]: {
            ...newField[DATASETS_IMPORT],
            isChecked
          },
          [IMPORTED_DATASET_NAME]: {
            name: IMPORTED_DATASET_NAME,
            value: '',
            message: isChecked ? MSG_REQUIRED : "",
            disabled: !(isChecked && fields.groupId.value !== "")
          }
        };
        optionsDatasets = getImportedDatasetsByGroupId(state.groups, fields.groupId.value, state.importedDatasets);
      }
      else if(GROUP === name){
        const datasetImport = state.fields[DATASETS_IMPORT];
        newField = {
          ...newField,
          [IMPORTED_DATASET_NAME]: {
            ...newField[IMPORTED_DATASET_NAME],
            value: '',
            message: datasetImport.isChecked ? MSG_REQUIRED : "",
            disabled: !(datasetImport.isChecked && event.value !== "")
          }
        }
        optionsDatasets = getImportedDatasetsByGroupId(state.groups, event.value, state.importedDatasets);
      }
      return {
        ...state,
        optionsDatasets,
        fields: newField
      };
    }
    case 'onSubmit': {
      const { status } = action.data;
      return {
        ...state,
        status,
        hasSubmit: true,
      };
    }
    default:
      throw new Error();
  }
};

export const init = (fetch, dispatch) => async () => {
  const datasetsPromise = fetchDatasets(fetch)();
  const groupsPromise = fetchGroups(fetch)(true);
  const importedDatasetsPromise = fetchImportedDatasets(fetch)();

  const [datasetsResponse, groupsResponse, importedDatasetsResponse] = await Promise.all([datasetsPromise, groupsPromise, importedDatasetsPromise]);
  let data;
  if(datasetsResponse.status >= 500 || groupsResponse.status >= 500 || importedDatasetsResponse.status >= 500) {
    data = { datasets: [], groups: [], importedDatasets: [], status: resilienceStatus.ERROR };
  } else {
    const datasets = await datasetsResponse.json();
    const groups = await groupsResponse.json();
    const importedDatasets = await importedDatasetsResponse.json();
    data = { datasets, groups, importedDatasets, status: resilienceStatus.SUCCESS };
  }
  dispatch( {type: 'init', data});
};

const useNew = (history, fetch, telemetry, environment) => {
  let iniStateReducer = initState;
  if(!environment.datasets.isBlobTransferActive) {
    iniStateReducer={...initState,fields: {
         ...initState.fields,
        [DATASETS_IMPORT]: {name: DATASETS_IMPORT, isChecked: false, message: "", isVisible:false},
        [IMPORTED_DATASET_NAME]: {name: IMPORTED_DATASET_NAME, value: '', disabled: true, message: MSG_REQUIRED, isVisible:false} }}
  }
  const [state, dispatch] = useReducer(reducer, iniStateReducer);
  const onChange = event => dispatch({ type: 'onChange', event });
  const onSubmit = async () => {
    const errors = errorList(state.fields);
    let data = {status: resilienceStatus.SUCCESS};
    if (!errors.length) {
      dispatch({type: 'onSubmit', data:{status: resilienceStatus.POST}});
      const fields = state.fields;
      const newDataset = {
        [NAME]: fields[NAME].value,
        [TYPE]: fields[TYPE].value,
        [GROUP]: fields[GROUP].value,
        [CLASSIFICATION]: fields[CLASSIFICATION].value
      }
      if(fields[DATASETS_IMPORT].isChecked){
        newDataset[IMPORTED_DATASET_NAME] = fields[IMPORTED_DATASET_NAME].value;
      }
      const response = await fetchCreateDataset(fetch)(newDataset);
      if(response.status >= 500){
        data = {status: resilienceStatus.ERROR };
      } else {
        telemetry.trackEvent(telemetryEvents.CREATE_DATASET);
        history.push('/datasets/confirm');
      }
    }
    dispatch({type: 'onSubmit', data});
  };

  React.useEffect(() => {
    init(fetch, dispatch)();
  }, []);
  
  return { state, onChange, onSubmit };
};

const NewWithResilience = withResilience(New);

export const NewContainer = ({ history, fetch, telemetry, environment }) => {
  const { state, onChange, onSubmit } = useNew(history, fetch, telemetry, environment);
   return <NewWithResilience {...state} onChange={onChange} onSubmit={onSubmit}  />;
};
const enhance = compose(withCustomFetch(fetch), withRouter, withTelemetry, withEnvironment);
export default enhance(NewContainer);
