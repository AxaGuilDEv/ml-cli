﻿import React from "react";
import {Header, Name, Title} from "@axa-fr/react-toolkit-layout-header";
import {Link} from 'react-router-dom';
import logo from '@axa-fr/react-toolkit-core/dist/assets/logo-axa.svg';
import './Home.scss';

const Home = () => (
    <div className="home">
        <Header>
            <Name
                title="ML-CLI"
                subtitle="Made by AXA"
                img={logo}
                alt="AXA Logo"
            />
        </Header>
        <Title title="Services list"/>
        <div className="home__links-container">
            <Link className="home__link" to="/compare">
                <div className="home__link-container home__link-container--compare">Services comparison</div>
            </Link>
            <Link className="home__link" to="/annotate">
                <div className="home__link-container home__link-container--annotate">Annotation</div>
            </Link>
        </div>
    </div>
);

export default Home;
