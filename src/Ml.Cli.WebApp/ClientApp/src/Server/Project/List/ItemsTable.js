﻿import HeaderColumnCell from "./ColumnHeader";
import React, {useState} from "react";
import {useHistory} from "react-router-dom";
import Table, { Paging } from '@axa-fr/react-toolkit-table';
import Loader from '@axa-fr/react-toolkit-loader';
import Action from '@axa-fr/react-toolkit-action';
import BooleanModal from '@axa-fr/react-toolkit-modal-boolean';
import {formatDateToString} from "../../date";

const ProjectRow = ({ id, name, classification, createDate, typeAnnotation, numberTagToDo, onDeleteProject }) => {
    const history = useHistory();
    const projectPageButton = id => {
        const path = `/projects/${id}`;
        history.push(path);
    };
    const [isDeleteModalVisible, setDeleteModalVisible] = useState(false);
    return (
        <Table.Tr key={id}>
            <Table.Td>{name}</Table.Td>
            <Table.Td>{classification}</Table.Td>
            <Table.Td>{createDate}</Table.Td>
            <Table.Td>{typeAnnotation}</Table.Td>
            <Table.Td>{numberTagToDo}</Table.Td>
            <Table.Td>
                <Action id="id" icon="zoom-in" title="Editer" onClick={() => projectPageButton(id)} />
                <Action id="removeActionId" icon="remove" title="Supprimer" onClick={() => setDeleteModalVisible(true)} />
                <DeleteProjectModal
                    idProject={id}
                    isDeleteModalVisible={isDeleteModalVisible}
                    setDeleteModalVisible={setDeleteModalVisible}
                    onDeleteProject={onDeleteProject}
                />
            </Table.Td>
        </Table.Tr>
    );
};

const DeleteProjectModal = ({ idProject, isDeleteModalVisible, setDeleteModalVisible, onDeleteProject }) => (
    <BooleanModal
        className={'af-modal'}
        classModifier={''}
        isOpen={isDeleteModalVisible}
        title={'Confirmer la suppression du projet ?'}
        id={'deleteModalId'}
        onCancel={() => setDeleteModalVisible(false)}
        onOutsideTap={() => setDeleteModalVisible(false)}
        onSubmit={() => {
            onDeleteProject(idProject);
            setDeleteModalVisible(false);
            return false;
        }}
        submitTitle={'Supprimer'}
        cancelTitle={'Annuler'}>
        <p>Confirmez-vous la suppression de ce projet ?</p>
    </BooleanModal>
);

const ItemsTable = ({items, filters, onChangePaging, onChangeSort, onDeleteProject}) => {
    
    return(
        <>
            <Table>
                <Table.Header>
                    <Table.Tr>
                        <HeaderColumnCell
                            onChangeSort={onChangeSort('name')}
                            headerColumnName={'Nom'}
                            filterColumnValue={filters.columns.name.value}
                        />
                        <HeaderColumnCell
                            onChangeSort={onChangeSort('classification')}
                            headerColumnName={'Classification'}
                            filterColumnValue={filters.columns.classification.value}
                        />
                        <HeaderColumnCell
                            onChangeSort={onChangeSort('createDate')}
                            headerColumnName={'Date création'}
                            filterColumnValue={filters.columns.createDate.value}
                        />
                        <HeaderColumnCell
                            onChangeSort={onChangeSort('typeAnnotation')}
                            headerColumnName={"Type d'annotation"}
                            filterColumnValue={filters.columns.typeAnnotation.value}
                        />
                        <HeaderColumnCell
                            onChangeSort={onChangeSort('numberTagToDo')}
                            headerColumnName={'Tags restants'}
                            filterColumnValue={filters.columns.numberCrossAnnotation.value}
                        />
                        <Table.Th classModifier="sortable">
                            <span className="af-table__th-content af-btn__text">Action</span>
                        </Table.Th>
                    </Table.Tr>
                </Table.Header>
                <Table.Body>
                    {items.map(({ id, name, classification, createDate, numberTagToDo, typeAnnotation }) => (
                        <ProjectRow
                            key={id}
                            id={id}
                            name={name}
                            classification={classification}
                            createDate={formatDateToString(createDate)}
                            numberTagToDo={numberTagToDo}
                            typeAnnotation={typeAnnotation}
                            onDeleteProject={onDeleteProject}
                        />
                    ))}
                </Table.Body>
            </Table>
            <Paging
                onChange={onChangePaging}
                numberItems={filters.paging.numberItemsByPage}
                numberPages={filters.paging.numberPages}
                currentPage={filters.paging.currentPage}
                id="home_paging"
            />
        </>
    )
    
};

export default ItemsTable;